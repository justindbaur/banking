using System.Text.Json;

namespace Banking.Abstractions;

public class StepResponse
{
    public StepResponse(JsonDocument answers, string? state)
    {
        Answers = answers;
        State = state;
    }

    public JsonDocument Answers { get; }
    public string? State { get; }
}
