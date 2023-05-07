using System.Text.Json;

namespace Banking.Abstractions.Entities;

public record Transaction
{
    public Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public virtual Account Account { get; init; } = default!;
    public required string TransactionId { get; init; }
    public required string Description { get; init; }
    public required DateTime Date { get; init; }
    public string? Category { get; init; }
    public required decimal Amount { get; init; }
    public string[]? Tags { get; init; }
    public JsonDocument? ExtraInfo { get; init; }
}
