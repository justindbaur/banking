using System.Diagnostics.CodeAnalysis;
using Banking.Abstractions;

namespace Banking.Api.Services.Implementations;

public class DefaultSourceProvider : ISourceProvider
{
    private readonly IEnumerable<ISource> _sources;


    public DefaultSourceProvider(IEnumerable<ISource> sources)
    {
        _sources = sources;

    }

    public bool TryGetSource(string sourceId, [MaybeNullWhen(false)] out ISource source)
    {
        source = _sources.FirstOrDefault(s => s.Id == sourceId);
        return source != null;
    }

}