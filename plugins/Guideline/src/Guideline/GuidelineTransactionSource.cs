using System.Text.Json;
using Banking.Abstractions.Entities;
using Microsoft.Extensions.Options;

namespace Banking.Plugin.Guideline;



public class GuidelineTransactionSource : ITransactionSource
{
    private readonly HttpClient _client;
    private readonly GuidelineOptions _guidelineOptions;
    private readonly JsonSerializerOptions _jsonOptions;

    public GuidelineTransactionSource(IHttpClientFactory httpClientFactory, IOptions<GuidelineOptions> optionsAccessor)
    {
        _client = httpClientFactory.CreateClient(Constants.ClientName);
        _guidelineOptions = optionsAccessor.Value;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public string SourceName => "Guideline";

    public async Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken cancellationToken)
    {


        throw new Exception("Testing");
    }

    public Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(Account account, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
