using System.Net.Http.Headers;

namespace Banking.Abstractions.Utilities;

public record AccessToken(string Scheme, string Value, TimeSpan Lifespan);

public record RefreshAccessTokenContext(HttpClient Client, AccessToken AccessToken);

public abstract class AuthenticatingMessageHandler : DelegatingHandler
{
    public static HttpRequestOptionsKey<bool> OptOut { get; } = new HttpRequestOptionsKey<bool>("OptOutOfAuthenticatingHandler");

    private readonly HttpClient _authenticatingClient;

    private DateTime? _lastRefreshed;
    private AccessToken? _accessToken;

    protected virtual TimeSpan RefreshAfter { get; }

    protected abstract Task<AccessToken> GetInitialAccessTokenAsync(HttpClient client, CancellationToken cancellationToken);

    protected virtual Task<AccessToken> GetRefreshAccessTokenAsync(RefreshAccessTokenContext context, CancellationToken cancellationToken)
        => GetInitialAccessTokenAsync(context.Client, cancellationToken);

    protected async virtual Task AuthenticateRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = await GetAuthenticationHeaderAsync(cancellationToken);
    }

    public AuthenticatingMessageHandler(HttpClient authenticatingClient)
    {
        _authenticatingClient = authenticatingClient;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return request.Options.TryGetValue(OptOut, out var shouldOptOut) && shouldOptOut
            ? base.SendAsync(request, cancellationToken)
            : SendCoreAsync(request, cancellationToken);
    }


    private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AuthenticateRequestAsync(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken);
    }

    private async ValueTask<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // If we have never gotten a token or it is past its lifetime
        if (!_lastRefreshed.HasValue || (_accessToken != null && _lastRefreshed + _accessToken.Lifespan < now))
        {
            _accessToken = await GetInitialAccessTokenAsync(_authenticatingClient, cancellationToken);
            _lastRefreshed = now;
            return new AuthenticationHeaderValue(_accessToken.Scheme, _accessToken.Value);
        }

        if (_lastRefreshed + RefreshAfter > now)
        {
            _accessToken = await GetRefreshAccessTokenAsync(new RefreshAccessTokenContext(_authenticatingClient, _accessToken!), cancellationToken);
            _lastRefreshed = now;
            return new AuthenticationHeaderValue(_accessToken.Scheme, _accessToken.Value);
        }

        // We know _lastRefreshed has a value which means _accessToken should also have a value
        return new AuthenticationHeaderValue(_accessToken!.Scheme, _accessToken.Value);
    }
}
