
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Banking.Abstractions.Utilities;

namespace Banking.Plugin.ConsumersCreditUnion;

public class AuthLoginRequest
{
    public AuthLoginRequest(Guid deviceId, string username, string password)
    {
        DeviceId = deviceId;
        Username = username;
        Password = password;
    }

    [JsonPropertyName("admin")]
    public bool Admin { get; }

    [JsonPropertyName("deviceId")]
    public Guid DeviceId { get; }

    [JsonPropertyName("username")]
    public string Username { get; }

    [JsonPropertyName("password")]
    public string Password { get; }
}

public class AuthLoginResponse
{
    public AuthLoginResponse(MfaOption[] mfaOptions, bool mfaRequired, string? resultCode)
    {
        MfaOptions = mfaOptions;
        MfaRequired = mfaRequired;
        ResultCode = resultCode;
    }

    [JsonPropertyName("mfaOptions")]
    public MfaOption[] MfaOptions { get; }

    [JsonPropertyName("mfaRequired")]
    public bool MfaRequired { get; }

    public string? ResultCode { get; }

    public class MfaOption
    {
        public MfaOption(string channel, string contactType, string description, bool? @default)
        {
            Channel = channel;
            ContactType = contactType;
            Description = description;
            Default = @default;
        }

        [JsonPropertyName("channel")]
        public string Channel { get; }

        [JsonPropertyName("contactType")]
        public string ContactType { get; }

        [JsonPropertyName("description")]
        public string Description { get; }

        [JsonPropertyName("default")]
        public bool? Default { get; }
    }
}

public class ConsumersCreditUnionCreator : ICreator
{
    private readonly HttpClient _httpClient;

    public ConsumersCreditUnionCreator(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ConsumersCreditUnion");
    }

    public async Task<ResumeToken> ResumeAsync(StepResponse stepResponse, CancellationToken cancellationToken = default)
    {
        if (stepResponse.State == null)
        {
            throw new ArgumentException("State is expected.");
        }

        var tracker = GetState<JsonDocument>(stepResponse.State);

        if (!tracker.RootElement.TryGetProperty("Step", out var stepElement) || stepElement.ValueKind != JsonValueKind.Number)
        {
            throw new ArgumentException("Expected step property in state.");
        }

        var step = stepElement.GetInt32();

        return step switch
        {
            0 => await DoLoginAsync(stepResponse, cancellationToken),
            1 => await DoOtpAsync(stepResponse, cancellationToken),
            _ => throw new ArgumentException("Unknown step number " + step),
        };

    }

    private async Task<ResumeToken> DoLoginAsync(StepResponse stepResponse, CancellationToken cancellationToken)
    {
        var answers = stepResponse.Answers;

        // TODO: This does not validate string, it should
        if (!answers.TryGetPropertyValue("username", out var usernameNode) || usernameNode == null)
        {
            throw new ArgumentException("Expected a username property of type string.");            
        }

        // TODO: This does not validate string, it should
        if (!answers.TryGetPropertyValue("password", out var passwordNode) || passwordNode == null)
        {
            throw new ArgumentException("Expected a password property of type string.");
        }

        var deviceId = Guid.NewGuid();

        var response = await _httpClient.PostAsJsonAsync("auth/login",
            new AuthLoginRequest(deviceId, usernameNode.GetValue<string>(), passwordNode.GetValue<string>()),
            cancellationToken
        );

        // sessionId cookie?

        var authLoginResponse = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(cancellationToken);

        if (authLoginResponse == null)
        {
            throw new ArgumentException("Expected object");
        }

        if (authLoginResponse.MfaRequired)
        {
            // Prefer code
            var codeOption = authLoginResponse.MfaOptions.FirstOrDefault(o => o.Channel == "GOOGLE_AUTH");

            if (codeOption != null)
            {
                var schema = new SchemaBuilder()
                    // Require 6 digit code to continue
                    .AddTextInput("code", "Authenticator Code")
                    .Build();

                return ResumeToken.Incomplete(schema, SetState(new
                {
                    Step = 1,
                    DeviceId = deviceId,
                }));
            }

            // TODO: Offer up all options
            throw new NotImplementedException("MFA is required but authenticator code was not an option.");
        }

        // We done?
        // Maybe resultCode === "ALREADY_LOGGED_IN"
        throw new NotImplementedException("MFA is not required");
    }

    private async Task<ResumeToken> DoOtpAsync(StepResponse stepResponse, CancellationToken cancellationToken)
    {
        Debug.Assert(stepResponse.State != null);
        var stateDocument = GetState<JsonDocument>(stepResponse.State);

        if (!stateDocument.RootElement.TryGetProperty("DeviceId", out var deviceIdElement) || deviceIdElement.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException("A device id should have been saved in the state.");
        }

        // TODO: Validate that it's a string
        if (!stepResponse.Answers.TryGetPropertyValue("code", out var codeNode) || codeNode == null)
        {
            throw new ArgumentException("A code answer was expected");
        }

        // resultCode property should be OK
        var response = await _httpClient.PostAsJsonAsync("auth/validatePreAuthOtp/GOOGLE_AUTH", new JsonObject
        {
            ["deviceId"] = deviceIdElement.GetGuid(),
            ["eventType"] = "WEB_LOGIN",
            ["otp"] = codeNode.GetValue<string>(),
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);

        if (responseDocument == null)
        {
            throw new Exception("Something");
        }

        if (!responseDocument.RootElement.TryGetProperty("resultCode", out var resultCodeElement) || resultCodeElement.ValueKind != JsonValueKind.String)
        {
            throw new Exception("Expected resultCode response");
        }

        if (resultCodeElement.GetString() != "OK")
        {
            throw new Exception("Did not receive the expected result code.");
        }

        // access_token property
        response = await _httpClient.PostAsJsonAsync("auth/stepUpLogin", new JsonObject(), cancellationToken);

        response.EnsureSuccessStatusCode();

        responseDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);

        if (responseDocument == null)
        {
            throw new Exception("Did not receive the expected response from auth/stepUpLogin");
        }

        if (!responseDocument.RootElement.TryGetProperty("access_token", out var accessTokenElement) || accessTokenElement.ValueKind != JsonValueKind.String)
        {
            throw new Exception("Did not receive the access token");
        }

        return ResumeToken.Complete(new JsonObject
        {
            ["accessToken"] = accessTokenElement.GetString(),
            // TODO: Validate this?
            ["deviceId"] = stateDocument.RootElement.GetProperty("DeviceId").GetGuid(),
        });
    }

    public async Task<StartToken> StartAsync(CancellationToken cancellationToken = default)
    {
        var schema = await ReadSchemaAsync("initial", cancellationToken);
        return new StartToken(schema, SetState(new { Step = 0 }));
    }

    public Task<AuthenticationStatus> GetStatusAsync(JsonDocument config)
    {
        throw new NotImplementedException();
    }

    private async Task<JsonDocument> ReadSchemaAsync(string name, CancellationToken cancellationToken)
    {
        var thisAssembly = Assembly.GetAssembly(GetType());
        Debug.Assert(thisAssembly != null, "Expected assembly to not be null");

        using var stream = thisAssembly.GetManifestResourceStream($"Banking.Plugin.ConsumersCreditUnion.Schemas.{name}.json");

        Debug.Assert(stream != null, "Expected stream to not be null.");

        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        Debug.Assert(jsonDocument != null, "JsonDocument should not parse to null.");

        return jsonDocument;
    }

    private T GetState<T>(string state)
    {
        // TODO: Use data protection APIs
        return JsonSerializer.Deserialize<T>(state)!;
    }

    private string SetState<T>(T value)
    {
        // TODO: Use data protection APIs
        return JsonSerializer.Serialize(value);
    }
}
