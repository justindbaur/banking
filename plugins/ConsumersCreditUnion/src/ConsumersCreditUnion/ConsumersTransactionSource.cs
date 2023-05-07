using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Banking.Abstractions.Entities;
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

    public async Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken cancellationToken)
    {
        // Get accounts
        var accountsResponse = await _client.GetFromJsonAsync<AccountsResponse>("gateway/web/accounts", _jsonOptions, cancellationToken);

        if (accountsResponse == null)
        {
            throw new Exception("Bad");
        }

        return accountsResponse.Accounts.Select(a => new Account
        {
            AccountId = a.Id.ToString(),
            Balance = a.ActualBalance,
            Name = a.Nickname,
            Enabled = true,
        });
    }

    public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(Account account, CancellationToken cancellationToken)
    {
        var exportResponse = await _client.PostAsJsonAsync($"gateway/web/exportTransactions/{account.AccountId}", new
        {
            account.AccountId,
            EndDate = DateTime.UtcNow,
            FileFormat = "CSV",
            IncludeExtendedTransactionData = true,
            StartDate = DateTime.UtcNow.AddYears(-10),
        }, cancellationToken);

        exportResponse.EnsureSuccessStatusCode();

        using var reader = new StreamReader(await exportResponse.Content.ReadAsStreamAsync(cancellationToken));
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csvReader.ReadAsync();
        csvReader.ReadHeader();

        var transactions = new List<Transaction>();
        while (await csvReader.ReadAsync())
        {
            JsonObject? extraInfo = null;
            if (csvReader.TryGetField("Check Number", out string? checkNumber) && !string.IsNullOrEmpty(checkNumber))
            {
                extraInfo = new JsonObject
                {
                    ["checkNumber"] = checkNumber,
                };
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
}
