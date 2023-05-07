using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Banking.Abstractions;
using Banking.Abstractions.Entities;
using Banking.Api.Services;
using Banking.Api.Services.Implementations;
using Banking.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication()
    .AddCookie(b =>
    {
        b.Cookie.Name = "Banking";

        b.SlidingExpiration = false;
        b.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

builder.Services.AddAuthorization(b =>
{
    b.AddPolicy("role:admin", p =>
    {
        p.RequireAuthenticatedUser();
        p.RequireRole(JwtClaimTypes.Role, "admin");
    });

    b.AddPolicy("read:account", p =>
    {
        p.RequireAuthenticatedUser();
        p.RequireClaim(JwtClaimTypes.Scope, "read:account");
    });
});

builder.Services.AddHttpClient("Passwordless", client =>
{
    client.BaseAddress = new Uri("https://api01.andersaberg.com");
    client.DefaultRequestHeaders.Add("ApiSecret", "banking:secret:2c461556270f4e72b6c4e0609099a953");
});

builder.Services.AddConsumersCreditUnion(builder.Configuration.GetSection("Consumers"));
builder.Services.AddGuideline(builder.Configuration.GetSection("Guideline"));

builder.Services.AddCors(options => 
{ 
    options.AddPolicy("Default", 
        policy => policy.WithOrigins("http://localhost:4200"));

    options.AddPolicy("AllowCredentials",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowCredentials()
            .AllowAnyHeader());
});

builder.Services.AddBankingContext();

builder.Services.AddScoped<ISyncService, DefaultSyncService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("hh:mm:ss"),
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("Default");

app.UseAuthorization();

app.MapGet("/sources", (IEnumerable<ITransactionSource> sources) => ListResponse.Create(sources.Select(s => s.SourceName).ToList()));

app.MapPost("/sync", async (ISyncService syncService, CancellationToken cancellationToken) =>
{
    await syncService.FullSyncAsync(cancellationToken);
    return NoContent();
});

app.MapPost("/sync/{sourceName}", async Task<IResult> (IEnumerable<ITransactionSource> transactionSources, string sourceName, CancellationToken cancellationToken) =>
{
    var transactionSource = transactionSources.FirstOrDefault(ts => ts.SourceName.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase));

    if (transactionSource == null)
    {
        return NotFound();
    }

    var accounts = await transactionSource.GetAccountsAsync(cancellationToken);

    foreach (var account in accounts)
    {

    }

    return NoContent();
});

app.MapGet("/transactions", async (BankingContext bankingContext, CancellationToken cancellationToken) =>
{
    var transactions = await bankingContext.Transactions.ToListAsync(cancellationToken);

    return ListResponse.Create(transactions);
});

app.MapGet("/transactions/{id}", async Task<IResult> (Guid id, BankingContext bankingContext) =>
{
    var transaction = await bankingContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
    return transaction switch
    {
        null => NotFound(),
        _ => Ok(transaction),
    };
});

app.MapPatch("/transaction/{id}", async Task<IResult> (Guid id, BankingContext bankingContext, TransactionUpdate transactionUpdate, CancellationToken cancellationToken) =>
{
    var transaction = await bankingContext.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    if (transaction == null)
    {
        return NotFound();
    }

    var patchedTransaction = transaction with
    {
        Category = transactionUpdate.Category,
    };

    bankingContext.Entry(transaction).CurrentValues.SetValues(patchedTransaction);

    await bankingContext.SaveChangesAsync(cancellationToken);
    return Ok(patchedTransaction);
});

app.MapGet("/accounts", async (BankingContext bankingContext, CancellationToken cancellationToken) =>
{
    var accounts = await bankingContext.Accounts.ToListAsync(cancellationToken);

    return ListResponse.Create(accounts);
});

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
    bankingContext.ApiKeys.Add(new ApiKey
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Scopes = request.Scopes,
        ExpirationDate = request.ExpirationDate,
        CreatedDate = now,
    });

    // TODO: Tell the user about the created token

    await bankingContext.SaveChangesAsync();

    var claims = new List<Claim>();
    foreach (var scope in request.Scopes)
    {
        // TODO: Validate each scope
        claims.Add(new Claim(JwtClaimTypes.Scope, scope));
    }

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),

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
    .RequireAuthorization();

app.MapGet("/jwt", async (BankingContext context) =>
{
    var keys = await context.ApiKeys.ToListAsync();
    return Ok(ListResponse.Create(keys));
})
    .RequireAuthorization("role:admin")
    .RequireCors("AllowCredentials");


app.Run();

record CreateJwtTokenRequest(string Name, string[] Scopes, DateTime? ExpirationDate);
record CreateTokenRequest(string FullName, string Username);
record LoginTokenRequest(string Token);
record TransactionUpdate(string Category);
record ListResponse<T>
{
    public ListResponse(List<T> data)
    {
        Data = data;
    }

    public int Count => Data.Count;
    public List<T> Data { get; }
}
static class ListResponse
{
    public static ListResponse<T> Create<T>(List<T> data) => new(data);
}
