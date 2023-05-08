using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Banking.Api.Authorization;

public static class AuthorizationOptionsExtensions
{
    public static void AddScopePolicy(this AuthorizationOptions authorizationOptions, string scope)
    {
        authorizationOptions.AddPolicy(scope, policy =>
        {
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(JwtClaimTypes.Scope, scope);
        });
    }

    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policy, string scope)
    {
        return policy.RequireClaim(JwtClaimTypes.Scope, scope);
    }

    public static AuthorizationPolicyBuilder AddManagementSchemes(this AuthorizationPolicyBuilder policy)
    {
        return policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, "ManagementKey");
    }
}
