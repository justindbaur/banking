using Banking.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankingContext(this IServiceCollection services)
    {
        services.AddDbContext<BankingContext>((sp, options) => 
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var sqliteConnectionString = config.GetSection("ConnectionStrings")["Sqlite"] ?? "Data Source=banking.db";
            options.UseSqlite(sqliteConnectionString);
        });

        return services;
    }
}
