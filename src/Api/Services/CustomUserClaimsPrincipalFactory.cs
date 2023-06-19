using System.Security.Claims;
using Banking.Abstractions.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Banking.Api.Services;

public class CustomUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<User>
{
    public Task<ClaimsPrincipal> CreateAsync(User user)
    {
        return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
            new Claim(JwtClaimTypes.Name, user.Username),
            new Claim(JwtClaimTypes.Scope, "admin"),
        }, authenticationType: "passwordless", nameType: JwtClaimTypes.Name, roleType: JwtClaimTypes.Role)));
    }
}
