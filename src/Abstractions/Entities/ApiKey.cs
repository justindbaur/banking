namespace Banking.Abstractions.Entities;

public record ApiKey
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string[] Scopes { get; init; }
    public required DateTime CreatedDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public DateTime? LastUsedDate { get; set; }
}
