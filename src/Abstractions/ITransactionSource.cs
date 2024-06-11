using System.Text.Json;
using Banking.Abstractions.Entities;

namespace Banking.Abstractions;

public interface ITransactionSource
{
    Task<IReadOnlyCollection<Account>> GetAccountsAsync(JsonDocument configuration, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Transaction>> GetAccountTransactionsAsync(JsonDocument configuration, Account account, CancellationToken cancellationToken);
}
