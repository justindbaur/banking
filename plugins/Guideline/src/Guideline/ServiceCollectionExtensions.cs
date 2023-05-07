using System.Net.Http.Headers;
using Banking.Plugin.Guideline;
using Banking.Plugin.Guideline.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGuideline(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GuidelineOptions>()
            .Bind(configuration);

        services.AddHttpClient(Constants.ClientName)
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GuidelineOptions>>().Value;
                client.BaseAddress = new Uri("https://my.guideline.com/api/v1/");

                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BankingApi", "1.0.0"));
                client.DefaultRequestHeaders.Add("X-GL-CLIENT", "web");
                client.DefaultRequestHeaders.Add("X-JS-UTC-OFFSET", "-300"); // Validate this
                client.DefaultRequestHeaders.Add("X-GL-UUID", options.Uuid);
                client.DefaultRequestHeaders.Add("Cookie", $"gl-uuid={options.Uuid}");
            })
            .AddHttpMessageHandler(sp => new GuidelineAuthenticatingHandler(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<IOptions<GuidelineOptions>>()
            ));


        services.AddHttpClient(GuidelineAuthenticatingHandler.ClientName)
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GuidelineOptions>>().Value;
                client.BaseAddress = new Uri("https://my.guideline.com/api/v1/");

                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BankingApi", "1.0.0"));
                client.DefaultRequestHeaders.Add("X-GL-CLIENT", "web");
                client.DefaultRequestHeaders.Add("X-JS-UTC-OFFSET", "-300"); // Validate this
                client.DefaultRequestHeaders.Add("X-GL-UUID", options.Uuid);
                client.DefaultRequestHeaders.Add("Cookie", $"gl-uuid={options.Uuid}");
            });

        services.AddSingleton<ITransactionSource, GuidelineTransactionSource>();

        return services;
    }
}
