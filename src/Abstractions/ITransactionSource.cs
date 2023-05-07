using Banking.Abstractions.Entities;

namespace Banking.Abstractions;

public interface ITransactionSource
{
    string SourceName { get; }
    Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(Account account, CancellationToken cancellationToken);
}
