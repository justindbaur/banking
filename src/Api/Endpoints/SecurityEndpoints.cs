using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Banking.Abstractions.Entities;
using Banking.Api.Authorization;
using Banking.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.AspNetCore.Http.Results;

namespace Banking.Api.Endpoints;

public static class SecurityEndpoints
{
    public static void MapSecurityEndpoints(this WebApplication app)
    {
        app.MapPost("/jwt", async (CreateJwtTokenRequest request, BankingContext bankingContext, HttpContext context) =>
        {
            var now = DateTime.UtcNow;
            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Scopes = request.Scopes,
                ExpirationDate = request.ExpirationDate,
                CreatedDate = now,
            };
            bankingContext.ApiKeys.Add(apiKey);

            // TODO: Tell the user about the created token

            await bankingContext.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, apiKey.Id.ToString())
            };
            foreach (var scope in request.Scopes)
            {
                // TODO: Validate each scope, maybe
                claims.Add(new Claim(JwtClaimTypes.Scope, scope));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                // TODO: Get key from settings or certificate define in settings
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey("my_super_duper_secure_test"u8.ToArray()), SecurityAlgorithms.HmacSha256),
                Issuer = "get-issuer",

                IssuedAt = now,
                NotBefore = now,
                Expires = request.ExpirationDate
            };

            var tokenHandler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false,
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new { access_token = tokenHandler.WriteToken(token) };
        })
            .RequireAuthorization(policy =>
            {
                policy.AddAuthenticationSchemes(
                    "ManagementKey",
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();
                policy.RequireScope(Scopes.Admin);
            })
            .RequireCors("AllowCredentials");

        app.MapGet("/jwt", async (BankingContext context) =>
        {
            var keys = await context.ApiKeys.ToListAsync();
            return Ok(ListResponse.Create(keys));
        })
            .RequireAuthorization(policy =>
            {
                policy.AddAuthenticationSchemes(
                    "ManagementKey",
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();
                policy.RequireScope(Scopes.Admin, Scopes.ListKeys);
            })
            .RequireCors("AllowCredentials");
    }
}
