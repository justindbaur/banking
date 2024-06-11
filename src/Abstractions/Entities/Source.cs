using System.Text.Json;

namespace Banking.Abstractions.Entities;

public record Source
{
    public Guid Id { get; init; }
    public required string SourceId { get; init; }
    public string? DisplayName { get; init; }
    public required JsonDocument Config { get; init; }
    public bool Enabled { get; init; }
}