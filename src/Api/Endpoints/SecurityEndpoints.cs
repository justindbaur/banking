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
        app.MapPost("/passwordless-register", async (CreateTokenRequest request, IHttpClientFactory clientFactory, BankingContext bankingContext) =>
        {
            var client = clientFactory.CreateClient("Passwordless");
            var userId = Guid.NewGuid();

            var response = await client.PostAsJsonAsync("/register/token", new
            {
                userId,
                displayName = request.FullName,
                username = request.Username,
            });

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();

            response = await client.PostAsJsonAsync("/alias", new
            {
                UserId = userId,
                aliases = new string[] { request.Username },
            });

            response.EnsureSuccessStatusCode();

            bankingContext.Users.Add(new User
            {
                Id = userId,
            });

            await bankingContext.SaveChangesAsync();

            return Ok(new { token });
        })
            .RequireCors("Default");

        app.MapPost("/passwordless-login", async (IHttpClientFactory httpClientFactory, HttpContext context, LoginTokenRequest request) =>
        {
            var client = httpClientFactory.CreateClient("Passwordless");

            var response = await client.PostAsJsonAsync("/signin/verify", new
            {
                token = request.Token,
            });

            using var jsonResponse = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (jsonResponse == null)
            {
                return BadRequest();
            }

            var root = jsonResponse.RootElement;

            if (!root.TryGetProperty("success", out var successElement) || !successElement.GetBoolean())
            {
                return BadRequest();
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, root.GetProperty("userId").GetString()!),
                new Claim(JwtClaimTypes.Role, "admin"),
            }, "passwordless", null, JwtClaimTypes.Role));

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return NoContent();
        })
            .RequireCors("AllowCredentials");

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
                policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, )
            })
            .RequireCors("AllowCredentials");
    }
}
