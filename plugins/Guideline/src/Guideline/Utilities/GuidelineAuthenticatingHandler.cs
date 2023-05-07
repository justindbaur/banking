using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Banking.Abstractions.Utilities;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Banking.Plugin.Guideline.Utilities;

public record MfaMethodsResponse(int Id, string Type);
public record OneTimePasswordResponse(
    [property: JsonPropertyName("mfa_methods")] MfaMethodsResponse[] MfaMethods,
    string[] Jwts
);
public record LoginResponse(OneTimePasswordResponse Otp);

public class GuidelineAuthenticatingHandler : AuthenticatingMessageHandler
{
    internal const string ClientName = "AuthenticatingGuideline";

    private readonly GuidelineOptions _guidelineOptions;
    private readonly JsonSerializerOptions _jsonOptions;

    public GuidelineAuthenticatingHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<GuidelineOptions> optionsAccessor
    )
        : base(httpClientFactory.CreateClient(ClientName))
    {
        _guidelineOptions = optionsAccessor.Value;

        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    protected override async Task<AccessToken> GetInitialAccessTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var loginResponse = await client.PostAsJsonAsync("sessions.json", new
        {
            _guidelineOptions.Email,
            _guidelineOptions.Password,
        }, _jsonOptions, cancellationToken);

        // error code: 1020
        // Fixed by adding cookie

        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions, cancellationToken);

        if (login is not { Otp.MfaMethods.Length: > 0 })
        {
            throw new NotImplementedException("Non-MFA is not currently handled.");
        }

        var mfaMethod = login.Otp.MfaMethods.FirstOrDefault(m => m.Type.Equals("Authenticator"));

        if (mfaMethod == null)
        {
            throw new Exception("Could not find a MFA method of 'Authenticator'");
        }

        var totp = new Totp(Base32Encoding.ToBytes(_guidelineOptions.SecretKey.Replace(" ", "")));
        var otp = totp.ComputeTotp();

        var verifyTwoFactorResponse = await client.PostAsJsonAsync("sessions/verify_otp.json", new
        {
            auth_method_id = mfaMethod.Id,
            code = otp,
            jwts = login.Otp.Jwts,
            remember = false, // TODO: I could probably remember
        }, _jsonOptions, cancellationToken);

        if (!verifyTwoFactorResponse.Headers.TryGetValues("x-jwt", out var jwtValues))
        {
            throw new Exception("No X-JWT headers");
        }

        var jwtValue = jwtValues.ToArray()[0];

        return new AccessToken("Bearer", jwtValue, TimeSpan.FromMinutes(10));
    }

    protected override async Task AuthenticateRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await base.AuthenticateRequestAsync(request, cancellationToken);
        request.Headers.Add("X-JWT-AUTH", "");
    }
}
