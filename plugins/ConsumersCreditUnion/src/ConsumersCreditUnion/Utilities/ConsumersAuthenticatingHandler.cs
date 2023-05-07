using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Banking.Abstractions.Utilities;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Banking.Plugin.ConsumersCreditUnion.Utilities;

internal record LoginResponse(MfaOption[] MfaOptions);

internal record MfaOption(string Channel);

internal record StepUpLoginResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

public class ConsumersAuthenticatingHandler : AuthenticatingMessageHandler
{
    internal const string ClientName = "AuthenticatingConsumersCreditUnion";

    private readonly ConsumersCreditUnionOptions _consumersOptions;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConsumersAuthenticatingHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<ConsumersCreditUnionOptions> optionsAccessor)
        : base(httpClientFactory.CreateClient(ClientName))
    {
        _consumersOptions = optionsAccessor.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    protected override async Task<AccessToken> GetInitialAccessTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var loginResponse = await client.PostAsJsonAsync("auth/login", new
        {
            Admin = false,
            _consumersOptions.DeviceId,
            _consumersOptions.Password,
            _consumersOptions.Username,
        }, _jsonOptions, cancellationToken);

        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions, cancellationToken);

        if (login is not { MfaOptions.Length: > 0 })
        {
            // Maybe handle for no 2FA
            throw new NotImplementedException("We do not handle for no 2FA right now.");
        }

        var googleAuth = login.MfaOptions.FirstOrDefault(o => o.Channel == "GOOGLE_AUTH");

        if (googleAuth == null)
        {
            throw new Exception("GOOGLE AUTH is required to be one of the options");
        }

        var totp = new Totp(Base32Encoding.ToBytes(_consumersOptions.SecretKey));
        var otp = totp.ComputeTotp(DateTime.UtcNow);

        // Go to GOOGLE_AUTH endpoint
        var otpResponse = await client.PostAsJsonAsync("auth/validatePreAuthOtp/GOOGLE_AUTH", new
        {
            _consumersOptions.DeviceId,
            Otp = otp,
        }, cancellationToken);

        otpResponse.EnsureSuccessStatusCode();

        var stepUpLoginResponse = await client.PostAsync("auth/stepUpLogin", null, cancellationToken);
        stepUpLoginResponse.EnsureSuccessStatusCode();

        var stepUpLogin = await stepUpLoginResponse.Content.ReadFromJsonAsync<StepUpLoginResponse>(_jsonOptions, cancellationToken);

        if (stepUpLogin == null)
        {
            throw new Exception("Bad");
        }

        return new AccessToken(
            stepUpLogin.TokenType,
            stepUpLogin.AccessToken,
            TimeSpan.FromSeconds(stepUpLogin.ExpiresIn));
    }

    protected override async Task<AccessToken> GetRefreshAccessTokenAsync(RefreshAccessTokenContext context, CancellationToken cancellationToken)
    {
        var (client, accessToken) = context;

        var response = await client.PostAsJsonAsync($"auth/refreshSession?memberRefreshTokenRequestId={Guid.NewGuid()}", new
        {
            Token = accessToken.Value,
        }, _jsonOptions, cancellationToken);

        var stepUpLogin = await response.Content.ReadFromJsonAsync<StepUpLoginResponse>(_jsonOptions, cancellationToken);

        if (stepUpLogin == null)
        {
            throw new Exception("Bad");
        }

        return new AccessToken(stepUpLogin.TokenType, stepUpLogin.AccessToken, TimeSpan.FromSeconds(stepUpLogin.ExpiresIn));
    }
}
