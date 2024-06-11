using System.Text.Json.Serialization;

namespace Banking.Plugin.ConsumersCreditUnion.Utilities;

public class ConsumersCreditUnionConfig
{
    public ConsumersCreditUnionConfig(string accessToken)
    {
        AccessToken = accessToken;
    }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; }
}