using System.Text.Json;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Storage;

public static class ModelBuilderExtensions
{
    public static PropertyBuilder<JsonDocument?> IsJson(this PropertyBuilder<JsonDocument?> propertyBuilder)
    {
        return propertyBuilder.HasConversion(
            d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
            s => JsonDocument.Parse(s, default)
        );
    }
}
