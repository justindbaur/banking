using System.Text.Json;

namespace Banking.Abstractions;

public class AuthenticationStatus
{
    public AuthenticationStatus(bool isExpired, string? state)
    {
        IsExpired = isExpired;
        State = state;
    }

    public bool IsExpired { get; }
    public string? State { get; }
}

public interface ICreator
{
    Task<StartToken> StartAsync(CancellationToken cancellationToken = default);
    Task<ResumeToken> ResumeAsync(StepResponse stepResponse, CancellationToken cancellationToken = default);
    Task<AuthenticationStatus> GetStatusAsync(JsonDocument configuration);
}
