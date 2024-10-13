using System.Text.Json;
using System.Text.Json.Nodes;
using Banking.Abstractions.Entities;

namespace Banking.Abstractions;

public interface ITransactionSource
{
    Task<IReadOnlyCollection<Account>> GetAccountsAsync(JsonNode configuration, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Transaction>> GetAccountTransactionsAsync(JsonNode configuration, Account account, CancellationToken cancellationToken);
}
