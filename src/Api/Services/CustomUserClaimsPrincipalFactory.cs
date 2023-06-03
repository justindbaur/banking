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
            new Claim(JwtClaimTypes.Role, "admin"),
        }, authenticationType: "passwordless", nameType: null, roleType: JwtClaimTypes.Role)));
    }
}
