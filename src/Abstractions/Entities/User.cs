namespace Banking.Abstractions.Entities;

public record User
{
    public Guid Id { get; init; }
    public string Username { get; set; } = null!;
}
