using System.Diagnostics;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Models;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddCookie(b =>
    {
        b.Cookie.Name = "Banking";

        if (builder.Environment.IsDevelopment()) 
        {
            b.Cookie.SameSite = SameSiteMode.None;
        }

        b.SlidingExpiration = false;

        if (builder.Environment.IsDevelopment())
        {
            b.ExpireTimeSpan = TimeSpan.FromHours(12);
        }
        else
        {
            b.ExpireTimeSpan = TimeSpan.FromHours(1);
        }
    })
    .AddJwtBearer(options =>
    {
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

builder.Services.AddScoped<IUserStore<User>, UserStore>();
builder.Services.AddSingleton<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();

builder.Services.AddPasswordless<User>(options =>
{
    var passwordlessConfig = builder.Configuration.GetSection("Passwordless");
    options.ApiSecret = passwordlessConfig.GetValue<string>("ApiSecret")
        ?? throw new InvalidProgramException("Passwordless:ApiSecret is a required configuration value.");

    options.SignInScheme = "Cookies";
});

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

app.MapGet("/sources", (IEnumerable<ISource> sources) => 
{ 
    return ListResponse.Create(sources.Select(s => new { s.Id, s.Name }).ToList());
})
    .RequireCors("AllowCredentials");

app.MapGet("/sources/{sourceId}/start", async Task<IResult> (IEnumerable<ISource> sources, string sourceId, CancellationToken cancellationToken) =>
{
    var source = sources.FirstOrDefault(s => s.Id == sourceId);

    if (source == null)
    {
        return ValidationProblem(new Dictionary<string, string[]>
        {

        }, "Invalid source");
    }

    var creator = source.Creator;

    var startToken = await creator.StartAsync(cancellationToken);

    return Ok(startToken);
})
    .RequireCors("AllowCredentials");

app.MapPost("/sources/{sourceId}/resume", async Task<IResult> (IEnumerable<ISource> sources, string sourceId, StepResponse stepResponse, CancellationToken cancellationToken) =>
{
    var source = sources.FirstOrDefault(s => s.Id == sourceId);

    if (source == null)
    {
        return ValidationProblem(
            new Dictionary<string, string[]>(),
            "Invalid source"
        );
    }

    var creator = source.Creator;

    var resumeToken = await creator.ResumeAsync(stepResponse, cancellationToken);

    return Ok(resumeToken);
})
    .RequireCors("AllowCredentials");

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
