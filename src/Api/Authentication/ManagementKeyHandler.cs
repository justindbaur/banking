using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Banking.Api.Authentication;

public class ManagementOptions
{
    public required string Key { get; set; }
}

public class ManagementKeyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private  readonly ManagementOptions _managementOptions;
    public ManagementKeyHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<ManagementOptions> optionsAccessor)
        : base(options, logger, encoder, clock)
    {
        _managementOptions = optionsAccessor.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("ApiKey", out var apiKeyHeaders))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var apiKey = apiKeyHeaders.First()!;

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(_managementOptions.Key),
            Encoding.UTF8.GetBytes(apiKey)))
        {
            return Task.FromResult(AuthenticateResult.Fail("The supplied management key is incorrect"));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new []
        {
            new Claim(JwtClaimTypes.Role, "admin"),
        }, "ManagementKey", null, JwtClaimTypes.Role));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, null, Scheme.Name)));
    }
}
