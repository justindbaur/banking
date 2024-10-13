using System.Text.Json.Nodes;

namespace Banking.Abstractions.Entities;

public record Source
{
    public Guid Id { get; init; }
    public required string SourceTemplateId { get; init; }
    public string? DisplayName { get; init; }
    public required JsonNode Config { get; init; }
    public DateTimeOffset LastUsed { get; set; }
    public bool Enabled { get; init; }
}