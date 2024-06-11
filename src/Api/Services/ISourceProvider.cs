using System.Diagnostics.CodeAnalysis;
using Banking.Abstractions;

namespace Banking.Api.Services;

public interface ISourceTemplateProvider
{
    bool TryGetSource(string sourceId, [MaybeNullWhen(false)] out ISourceService source);
}