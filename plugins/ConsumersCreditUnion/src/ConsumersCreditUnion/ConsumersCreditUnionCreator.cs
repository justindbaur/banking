
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Banking.Plugin.ConsumersCreditUnion;

public class ConsumersCreditUnionCreator : ICreator
{
    public Task<ResumeToken> ResumeAsync(StepResponse stepResponse, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<StartToken> StartAsync(CancellationToken cancellationToken = default)
    {
        var schema = await ReadSchemaAsync("initial", cancellationToken);

        return new StartToken(schema, null);
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
}
