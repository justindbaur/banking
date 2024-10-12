using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Banking.Abstractions;

public class ResumeToken
{
    private readonly JsonNode _jsonNode;

    private ResumeToken(bool isComplete, JsonNode jsonNode, string? state)
    {
        IsComplete = isComplete;
        _jsonNode = jsonNode;
        State = state;
    }

    [MemberNotNullWhen(false, nameof(RequirementsSchema))]
    [MemberNotNullWhen(true, nameof(FinalConfiguration))]
    public bool IsComplete { get; }

    public JsonNode? RequirementsSchema
    {
        get { return IsComplete == false ? _jsonNode : null; }
    }

    public JsonNode? FinalConfiguration
    {
        get { return IsComplete == true ? _jsonNode : null; }
    }

    public string? State { get; }

    public static ResumeToken Complete(JsonNode finalConfiguration)
    {
        return new ResumeToken(true, finalConfiguration, null);
    }

    public static ResumeToken Incomplete(JsonNode requirementsSchema, string? state)
    {
        return new ResumeToken(false, requirementsSchema, state);
    }
}
