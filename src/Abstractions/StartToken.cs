using System.Text.Json;

namespace Banking.Abstractions;

public class StartToken
{
    public StartToken(JsonDocument requirementsSchema, string? state)
    {
        RequirementsSchema = requirementsSchema;
        State = state;
    }

    public JsonDocument RequirementsSchema { get; }
    public string? State { get; }
}
