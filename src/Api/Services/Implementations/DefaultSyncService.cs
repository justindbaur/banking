using Banking.Abstractions;
using Banking.Storage;

using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Services.Implementations;

public class DefaultSyncService : ISyncService
{
    private readonly BankingContext _bankingContext;
    private readonly IEnumerable<ITransactionSource> _transactionSources;

    public DefaultSyncService(BankingContext bankingContext, IEnumerable<ITransactionSource> transactionSources)
    {
        _bankingContext = bankingContext;
        _transactionSources = transactionSources;
    }

    public async Task FullSyncAsync(CancellationToken cancellationToken)
    {
        foreach (var transactionSource in _transactionSources)
        {
            var accounts = await transactionSource.GetAccountsAsync(cancellationToken);

            foreach (var account in accounts)
            {
                var existingAccount = await _bankingContext.Accounts.FirstOrDefaultAsync(a => a.AccountId == account.AccountId, cancellationToken);

                if (existingAccount == null)
                {
                    _bankingContext.Accounts.Add(account);
                }

                await _bankingContext.SaveChangesAsync(cancellationToken);

                var transactions = await transactionSource.GetAccountTransactionsAsync(account, cancellationToken);

                foreach (var transaction in transactions)
                {
                    var existingTransaction = _bankingContext.Transactions.FirstOrDefault(t => t.TransactionId == transaction.TransactionId);

                    if (existingTransaction == null)
                    {
                        _bankingContext.Transactions.Add(transaction);
                    }
                }
            }

            await _bankingContext.SaveChangesAsync(cancellationToken);
        }
    }
}
