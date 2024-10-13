using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Banking.Api.Repositories.Yaml;
using YamlDotNet.Serialization;

namespace Banking.Api.Tests.Repositories.Yaml;

public class JsonYamlConverterTests
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public JsonYamlConverterTests()
    {
        var converter = new JsonYamlConverter();

        _serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
    }

    public static IEnumerable<object[]> Data()
    {

        yield return D("{}", "{}\n"); // Empty object
        yield return D("{\"property\":true}", "property: true\n"); // Single property object
        yield return D("\"Hi\"", "'Hi'\n"); // String
        yield return D("[\"Hi\",true,false,14]", "- 'Hi'\n- true\n- false\n- 14\n"); // Array
        yield return D("true", "true\n"); // True literal
        yield return D("false", "false\n"); // False literal
        // yield return D("null", "--- \n");

        static object[] D([StringSyntax(StringSyntaxAttribute.Json)] string json, string yaml)
        {
            return [json, yaml];
        }
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteYaml_JsonDocument_Works(string json, string yaml)
    {
        using var jd = JsonDocument.Parse(json);
        var serializedJson = _serializer.Serialize(jd);
        Assert.Equal(yaml, serializedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteYaml_JsonElement_Works(string json, string yaml)
    {
        using var jd = JsonDocument.Parse(json);
        var serializedJson = _serializer.Serialize(jd.RootElement);
        Assert.Equal(yaml, serializedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteYaml_JsonNode_Works(string json, string yaml)
    {
        var jsonNode = JsonNode.Parse(json);
        var serializedJson = _serializer.Serialize(jsonNode);
        Assert.Equal(yaml, serializedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadYaml_JsonDocument_Works(string json, string yaml)
    {
        using var jd = _deserializer.Deserialize<JsonDocument>(yaml);
        var returnedJson = JsonSerializer.Serialize(jd);
        Assert.Equal(json, returnedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadYaml_JsonElement_Works(string json, string yaml)
    {
        using var jd = _deserializer.Deserialize<JsonDocument>(yaml);
        var returnedJson = JsonSerializer.Serialize(jd.RootElement);
        Assert.Equal(json, returnedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadYaml_JsonNode_Works(string json, string yaml)
    {
        var jsonNode = _deserializer.Deserialize<JsonNode>(yaml);
        var returnedJson = jsonNode.ToJsonString();
        Assert.Equal(json, returnedJson);
    }
}