namespace Banking.Abstractions.Entities;

public record User
{
    public Guid Id { get; init; }
    public string UserName { get; set; } = null!;
}
