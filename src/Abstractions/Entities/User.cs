namespace Banking.Abstractions.Entities;

public record User
{
    public required Guid Id { get; init; }
}
