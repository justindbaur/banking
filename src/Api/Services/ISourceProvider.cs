using System.Diagnostics.CodeAnalysis;
using Banking.Abstractions;

namespace Banking.Api.Services;

public interface ISourceProvider
{
    bool TryGetSource(string sourceId, [MaybeNullWhen(false)] out ISource source);
}