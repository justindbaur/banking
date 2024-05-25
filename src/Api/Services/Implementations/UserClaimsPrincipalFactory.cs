using System.Security.Claims;
using Banking.Abstractions.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Banking.Api.Services.Implementations;

public class UserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<User>
{
    public Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var claims = new Claim[]
        {
            new(JwtClaimTypes.Id, user.Id.ToString(), ClaimValueTypes.String),
            new(JwtClaimTypes.Name, user.UserName, ClaimValueTypes.String),
        };

        // TODO: Do I need to add an AuthenticationType?
        return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(claims, "authed")));
    }
}
