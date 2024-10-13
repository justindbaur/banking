using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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

        yield return D("{}", "{}\n");
        yield return D("{\"property\":true}", "property: true\n");
        yield return D("\"Hi\"", "'Hi'\n");
        yield return D("[\"Hi\",true,false,14]", "- 'Hi'\n- true\n- false\n- 14\n");
        yield return D("true", "true\n");
        yield return D("false", "false\n");

        static object[] D([StringSyntax(StringSyntaxAttribute.Json)] string json, string yaml)
        {
            return [json, yaml];
        }
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteYaml_Works(string json, string yaml)
    {
        using var jd = JsonDocument.Parse(json);
        var serializedJson = _serializer.Serialize(jd);
        Assert.Equal(yaml, serializedJson);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadYaml_Works(string json, string yaml)
    {
        using var jd = _deserializer.Deserialize<JsonDocument>(yaml);
        var returnedJson = JsonSerializer.Serialize(jd);
        Assert.Equal(json, returnedJson);
    }
}