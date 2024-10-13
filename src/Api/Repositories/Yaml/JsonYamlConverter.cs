using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Banking.Api.Repositories.Yaml;

public class JsonYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(JsonDocument) ||
            type == typeof(JsonElement) ||
            type.IsAssignableTo(typeof(JsonNode));
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var jsonNode = ReadYamlCore(parser);

        if (type.IsAssignableTo(typeof(JsonNode)))
        {
            // Do we need to merge this into the upper type?
            return jsonNode;
        }

        var jsonDocument = JsonSerializer.SerializeToDocument(jsonNode);
        return type == typeof(JsonDocument)
            ? jsonDocument
            : jsonDocument.RootElement;
    }

    public JsonNode? ReadYamlCore(IParser parser)
    {
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            if (scalar.Style == ScalarStyle.SingleQuoted)
            {
                return (JsonValue)scalar.Value;
            }
    
            return JsonNode.Parse(scalar.Value);
        }
        else if (parser.TryConsume<MappingStart>(out _))
        {
            var jsonObject = new JsonObject();
            while (parser.TryConsume<Scalar>(out var propertyScalar))
            {
                jsonObject.Add(propertyScalar.Value, ReadYamlCore(parser));
            }

            if (!parser.TryConsume<MappingEnd>(out _))
            {
                throw new UnreachableException("Expected a MappingEnd event");
            }

            return jsonObject;
        }
        else if (parser.TryConsume<SequenceStart>(out _))
        {
            // Array
            var jsonArray = new JsonArray();

            while (parser.Accept<Scalar>(out _))
            {
                jsonArray.Add(ReadYamlCore(parser));
            }

            parser.Require<SequenceEnd>();
            parser.MoveNext();
            
            return jsonArray;
        }
        else
        {
            if (parser.MoveNext())
            {
                var currentEventType = parser.Current!.GetType();
                throw new NotSupportedException($"Cannot read event of type '{currentEventType.FullName}'");
            }

            throw new InvalidOperationException("Nothing to consume.");
        }

        throw new UnreachableException();
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        JsonElement jsonElement;
        if (value is JsonNode jsonNode)
        {
            jsonElement = jsonNode.Deserialize<JsonElement>();
        }
        else
        {
            jsonElement = value is JsonDocument jsonDocument
                ? jsonDocument.RootElement
                : (JsonElement)value!;
        }

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.True:
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, "true"));
                break;
            case JsonValueKind.False:
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, "false"));
                break;
            case JsonValueKind.Number:
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, jsonElement.GetRawText()));
                break;
            case JsonValueKind.String:
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, jsonElement.GetString()!, ScalarStyle.SingleQuoted, true, true));
                break;
            case JsonValueKind.Object:
                emitter.Emit(new MappingStart());
                foreach (var property in jsonElement.EnumerateObject())
                {
                    emitter.Emit(new Scalar(property.Name));
                    WriteYaml(emitter, property.Value, typeof(JsonElement));
                }
                emitter.Emit(new MappingEnd());
                break;
            case JsonValueKind.Array:
                emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Block));
                foreach (var arrayElement in jsonElement.EnumerateArray())
                {
                    WriteYaml(emitter, arrayElement, typeof(JsonElement));
                }
                emitter.Emit(new SequenceEnd());
                break;
        }
    }
}