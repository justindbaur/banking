using System.Net.Http.Headers;
using System.Reflection;
using Banking.Plugin.Guideline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Guideline.Tests;

public class GuidelineTransactionSourceTests
{
    public IConfiguration Configuration { get; }
    private readonly Mock<GuidelineOptions> _mockOptions;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    private readonly GuidelineTransactionSource _sut;

    public GuidelineTransactionSourceTests()
    {
        var testClient = new HttpClient
        {
            BaseAddress = new Uri("https://my.guideline.com/api/v1/"),
        };

        testClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        testClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        testClient.DefaultRequestHeaders.Add("X-GL-CLIENT", "web");
        testClient.DefaultRequestHeaders.Add("X-JS-UTC-OFFSET", "-300");
        // testClient.DefaultRequestHeaders.Add("X-GL-UUID", "");
        // testClient.DefaultRequestHeaders.Add("Cookie", "");

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        _mockHttpClientFactory
            .Setup(f => f.CreateClient(Constants.ClientName))
            .Returns(testClient);
        _mockOptions = new Mock<GuidelineOptions>();

        Configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.Load("Api"))
            .Build().GetSection("Guideline");

        _sut = new GuidelineTransactionSource(_mockHttpClientFactory.Object, Options.Create(_mockOptions.Object));
    }

    [Fact]
    public async Task GetAccountsAsync()
    {
        // var email = Configuration.GetValue<string>("Email")!;
        // _mockOptions
        //     .Setup(o => o.Email)
        //     .Returns(email);

        // var password = Configuration.GetValue<string>("Password")!;
        // _mockOptions
        //     .Setup(o => o.Password)
        //     .Returns(password);

        // var secretKey = Configuration.GetValue<string>("SecretKey")!;
        // _mockOptions
        //     .Setup(o => o.SecretKey)
        //     .Returns(secretKey);

        // var accounts = await _sut.GetAccountsAsync(CancellationToken.None);

        // Assert.NotNull(_sut);
        await Task.CompletedTask;
    }
}
