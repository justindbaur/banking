using System.Text.Json;
using System.Text.Json.Nodes;

namespace Banking.Abstractions;

public class StepResponse
{
    public StepResponse(JsonObject answers, string? state)
    {
        Answers = answers;
        State = state;
    }

    public JsonObject Answers { get; }
    public string? State { get; }
}
