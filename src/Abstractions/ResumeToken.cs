using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Banking.Abstractions;

public class ResumeToken
{
    private readonly JsonDocument _jsonDocument;

    private ResumeToken(bool isComplete, JsonDocument jsonDocument, string? state)
    {
        IsComplete = isComplete;
        _jsonDocument = jsonDocument;
        State = state;
    }

    [MemberNotNullWhen(false, nameof(RequirementsSchema))]
    [MemberNotNullWhen(true, nameof(FinalConfiguration))]
    public bool IsComplete { get; }

    public JsonDocument? RequirementsSchema
    {
        get { return IsComplete == false ? _jsonDocument : null; }
    }

    public JsonDocument? FinalConfiguration
    {
        get { return IsComplete == true ? _jsonDocument : null; }
    }

    public string? State { get; }

    public static ResumeToken Complete(JsonDocument finalConfiguration)
    {
        return new ResumeToken(true, finalConfiguration, null);
    }

    public static ResumeToken Incomplete(JsonDocument requirementsSchema, string? state)
    {
        return new ResumeToken(false, requirementsSchema, state);
    }
}
