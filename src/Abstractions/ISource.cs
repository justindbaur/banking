namespace Banking.Abstractions;

public interface ISource
{
    string Id { get; }
    string Name { get; }
    ICreator Creator { get; }
    ITransactionSource? TransactionSource { get; }
}
