using System.Text.Json;

namespace Banking.Abstractions.Entities;

public record Account
{
    public Guid Id { get; init; }
    public required string AccountId { get; init; }
    public required string Name { get; init; }
    public required decimal Balance { get; init; }
    public bool Enabled { get; init; }
    public JsonDocument? ExtraInfo { get; init; }
    public virtual ICollection<Transaction> Transactions { get; init; } = default!;
}
