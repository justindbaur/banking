using System.Net.Http.Headers;
using Banking.Plugin.ConsumersCreditUnion;
using Banking.Plugin.ConsumersCreditUnion.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsumersCreditUnion(this IServiceCollection services)
    {
        services.AddHttpClient(Constants.HttpClientName)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(Constants.BaseUrl);
                // Consumers requires a User-Agent header
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BankingApi", "1.0.0"));
            });

        services.AddSingleton<ISourceService, ConsumersCreditUnionSource>();

        return services;
    }
}
