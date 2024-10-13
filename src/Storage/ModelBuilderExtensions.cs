using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Storage;

public static class ModelBuilderExtensions
{
    public static PropertyBuilder<JsonDocument> IsJson(this PropertyBuilder<JsonDocument> propertyBuilder)
    {
        return propertyBuilder.HasConversion(
            d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
            s => JsonDocument.Parse(s, default)
        );
    }

    public static PropertyBuilder<JsonNode> IsJson(this PropertyBuilder<JsonNode> propertyBuilder)
    {
        return propertyBuilder.HasConversion(
            d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
            s => JsonNode.Parse(s, (JsonNodeOptions?)null, (JsonDocumentOptions)default)!
        );
    }

    public static PropertyBuilder<JsonDocument?> IsNullableJson(this PropertyBuilder<JsonDocument?> propertyBuilder)
    {
        return propertyBuilder.HasConversion(
            d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
            s => JsonDocument.Parse(s, default)
        );
    }
}
