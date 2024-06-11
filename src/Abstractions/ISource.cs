namespace Banking.Abstractions;

public interface ISourceService
{
    string Id { get; }
    string Name { get; }
    ICreator Creator { get; }
    ITransactionSource? TransactionSource { get; }
}
