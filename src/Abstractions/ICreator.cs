using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
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

public class ResumeToken
{
    private readonly JsonDocument _jsonDocument;

    private ResumeToken(bool isComplete, JsonDocument jsonDocument, string? state)
    {
        IsComplete = isComplete;
        _jsonDocument = jsonDocument;
        State = state;
    }

    public bool IsComplete { get; }

    [MemberNotNullWhen(false, nameof(IsComplete))]
    public JsonDocument? RequirementsSchema
    {
        get { return IsComplete == false ? _jsonDocument : null; }
    }

    [MemberNotNullWhen(true, nameof(IsComplete))]
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

public interface ICreator
{
    Task<StartToken> StartAsync(CancellationToken cancellationToken = default);
    Task<ResumeToken> ResumeAsync(StepResponse stepResponse, CancellationToken cancellationToken = default);
}
