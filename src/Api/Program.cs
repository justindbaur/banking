using System.Security.Claims;
using Banking.Abstractions;
using Banking.Abstractions.Entities;
using Banking.Api.Authentication;
using Banking.Api.Authorization;
using Banking.Api.Endpoints;
using Banking.Api.Services;
using Banking.Api.Services.Implementations;
using Banking.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Passwordless.AspNetCore;
using Passwordless.AspNetCore.Services;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(options => 
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(b =>
    {
        b.Cookie.Name = "Banking";

        b.Cookie.SameSite = SameSiteMode.None;
        b.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        b.SlidingExpiration = false;
        b.ExpireTimeSpan = TimeSpan.FromHours(1);
    })
    .AddJwtBearer(options =>
    {
        options.ForwardChallenge = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var apiKeyIdString = context.Principal?.FindFirstValue(JwtClaimTypes.JwtId);
                if (string.IsNullOrEmpty(apiKeyIdString) || !Guid.TryParse(apiKeyIdString, out var apiKeyId))
                {
                    context.Fail("Missing required property");
                    return;
                }

                var bankingContext = context.HttpContext.RequestServices.GetRequiredService<BankingContext>();
                var foundKey = await bankingContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == apiKeyId);
                if (foundKey == null)
                {
                    context.Fail("Key has expired.");
                    return;
                }

                foundKey.LastUsedDate = DateTime.UtcNow;
                await bankingContext.SaveChangesAsync();
            },
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ManagementKeyHandler>("ManagementKey", "Management Key", null);

builder.Services.Configure<ManagementOptions>(builder.Configuration.GetSection("Management"));

builder.Services.AddAuthorization(b =>
{
    b.AddScopePolicy(Scopes.Admin);
    b.AddScopePolicy(Scopes.Sync);
});

builder.Services.AddPasswordless<User>(builder.Configuration.GetRequiredSection("Passwordless"));

builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomUserClaimsPrincipalFactory>();
builder.Services.AddScoped<IUserStore<User>, UserStore>();

builder.Services.AddScoped<IRegisterService<CustomRegisterRequest>, CustomRegisterService>();


// builder.Services.AddHttpClient("Passwordless-1", (services, client) =>
// {
//     var config = services.GetRequiredService<IConfiguration>();

//     var passwordlessSection = config.GetRequiredSection("Passwordless");
//     var apiUrl = passwordlessSection.GetValue<string>("ApiUrl")
//         ?? throw new InvalidOperationException("Missing Passwordless:ApiUrl in config");

//     var apiSecret = passwordlessSection.GetValue<string>("ApiSecret")
//         ?? throw new InvalidOperationException("Missing Passwordless:ApiSecret in config");

//     client.BaseAddress = new Uri(apiUrl);
//     client.DefaultRequestHeaders.Add("ApiSecret", apiSecret);
// });

builder.Services.AddConsumersCreditUnion(builder.Configuration.GetSection("Consumers"));
builder.Services.AddGuideline(builder.Configuration.GetSection("Guideline"));

builder.Services.AddCors(options => 
{ 
    options.AddPolicy("Default", 
        policy => policy.WithOrigins("http://localhost:4200"));

    options.AddPolicy("AllowCredentials",
        policy => policy
            .WithOrigins("http://localhost:4200", "http://localhost:5026")
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

    var managementKeySchema = new OpenApiSecurityScheme
    {
        Description = "Management Key for generating tokens",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Name = "ApiKey",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ManagementKey",
        }
    };

    options.AddSecurityDefinition("ManagementKey", managementKeySchema);

    var bearerScheme = new OpenApiSecurityScheme
    {
        Description = "Authorization token. Example: \"Bearer {apikey}\"",
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer",
        }
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            managementKeySchema,
            Array.Empty<string>()
        },
        {
            bearerScheme,
            Array.Empty<string>()
        }
    });

    options.SupportNonNullableReferenceTypes();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", async (BankingContext dbContext) =>
    {
        await dbContext.Database.MigrateAsync();
        return "Ran migrations for you";
    });

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

app.MapPasswordless<CustomRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>(new PasswordlessEndpointOptions
{
    AddCredentialPath = null,
})
    .RequireCors("AllowCredentials");

app.MapSecurityEndpoints();

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
