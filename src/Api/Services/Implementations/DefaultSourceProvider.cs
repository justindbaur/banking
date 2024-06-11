using System.Diagnostics.CodeAnalysis;
using Banking.Abstractions;

namespace Banking.Api.Services.Implementations;

public class DefaultSourceProvider : ISourceTemplateProvider
{
    private readonly IEnumerable<ISourceService> _sources;


    public DefaultSourceProvider(IEnumerable<ISourceService> sources)
    {
        _sources = sources;

    }

    public bool TryGetSource(string sourceId, [MaybeNullWhen(false)] out ISourceService source)
    {
        source = _sources.FirstOrDefault(s => s.Id == sourceId);
        return source != null;
    }

}