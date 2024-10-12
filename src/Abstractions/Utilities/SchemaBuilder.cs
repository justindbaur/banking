using System.Text.Json;
using System.Text.Json.Nodes;

namespace Banking.Abstractions.Utilities;

public class SchemaBuilder
{
    private readonly JsonObject _inputs;

    public SchemaBuilder()
    {
        _inputs = [];
    }

    public SchemaBuilder AddTextInput(string propertyName, string label, int order = 0)
    {
        _inputs.Add(propertyName, new JsonObject
        {
            ["type"] = "text",
            ["label"] = label,
            ["order"] = order,
        });
        return this;
    }

    public SchemaBuilder AddPasswordInput(string propertyName, string label, int order = 0)
    {
        _inputs.Add(propertyName, new JsonObject
        {
            ["type"] = "password",
            ["label"] = label,
            ["order"] = order,
        });
        return this;
    }

    public SchemaBuilder AddCustomInput(string propertyName, JsonObject input)
    {
        _inputs.Add(propertyName, input);
        return this;
    }

    public JsonObject Build()
    {
        return new JsonObject
        {
            ["inputs"] = _inputs,
        };
    }
}