using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Banking.Abstractions.Entities;
using Banking.Plugin.ConsumersCreditUnion.Utilities;
using CsvHelper;

namespace Banking.Plugin.ConsumersCreditUnion;

internal record ConsumersAccount(Guid Id, decimal ActualBalance, decimal AvailableBalance, string Nickname, string Type);
internal record AccountsResponse(ConsumersAccount[] Accounts);

internal class ConsumersTransactionSource : ITransactionSource
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConsumersTransactionSource(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.HttpClientName);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public string SourceName => "ConsumersCreditUnion";

    public async Task<IReadOnlyCollection<Account>> GetAccountsAsync(JsonNode configuration, CancellationToken cancellationToken)
    {
        var config = ParseConfig(configuration);

        // Get accounts
        var request = new HttpRequestMessage(HttpMethod.Get, "gateway/web/accounts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AccessToken);

        var response = await _client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var accounts = await response.Content.ReadFromJsonAsync<AccountsResponse>(cancellationToken);

        if (accounts == null)
        {
            throw new Exception("Expected non-null response");
        }

        return accounts.Accounts.Select(a => new Account
        {
            AccountId = a.Id.ToString(),
            Balance = a.ActualBalance,
            Name = a.Nickname,
            Enabled = true,
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAccountTransactionsAsync(JsonNode configuration, Account account, CancellationToken cancellationToken)
    {
        var config = ParseConfig(configuration);
        var request = new HttpRequestMessage(HttpMethod.Post, $"gateway/web/exportTransactions/{account.AccountId}")
        {
            Content = JsonContent.Create(new
            {
                account.AccountId,
                EndDate = DateTime.UtcNow,
                FileFormat = "CSV",
                IncludeExtendedTransactionData = true,
                StartDate = DateTime.UtcNow.AddYears(-10),
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.AccessToken);

        var response = await _client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken));
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csvReader.ReadAsync();
        csvReader.ReadHeader();

        var transactions = new List<Transaction>();
        while (await csvReader.ReadAsync())
        {
            var extraInfo = new JsonObject();
            if (csvReader.TryGetField("Check Number", out string? checkNumber) && !string.IsNullOrEmpty(checkNumber))
            {
                extraInfo.Add("checkNumber", checkNumber);
            }

            var transaction = new Transaction
            {
                TransactionId = csvReader.GetField<string>("Transaction ID") ?? "",
                AccountId = account.Id,
                Date = csvReader.GetField<DateTime>("Date"),
                Description = csvReader.GetField<string>("Description") ?? "",
                Amount = decimal.Parse(csvReader.GetField<string>("Amount")?.Replace("$", "") ?? "0"),
                Category = csvReader.GetField<string>("Category"),
                ExtraInfo = extraInfo != null ? JsonSerializer.SerializeToDocument(extraInfo) : null,
            };

            transactions.Add(transaction);
        }

        return transactions;
    }

    private static ConsumersCreditUnionConfig ParseConfig(JsonNode jsonDocument)
    {
        var config = jsonDocument.Deserialize<ConsumersCreditUnionConfig>();

        if (config == null)
        {
            throw new Exception("Invalid config");
        }

        return config;
    }
}
