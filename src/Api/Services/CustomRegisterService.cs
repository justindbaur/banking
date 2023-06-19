using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.AspNetCore.Services;
using Passwordless.Net;

namespace Banking.Api.Services;

public class CustomRegisterService : IRegisterService<CustomRegisterRequest>
{
    public Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> RegisterAsync(CustomRegisterRequest body, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public record CustomRegisterRequest : PasswordlessRegisterRequest
{
    public CustomRegisterRequest(string Email, string Username, string? DisplayName, HashSet<string>? Aliases) : base(Username, DisplayName, Aliases)
    {
    }
}
