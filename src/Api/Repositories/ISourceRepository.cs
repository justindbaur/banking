using System.Text.Json;
using Banking.Abstractions.Entities;

namespace Banking.Api.Repositories;

public record CreateSource(string SourceTemplateId, string DisplayName, JsonDocument Config);

public interface ISourceRepository
{
    Task<Guid> CreateAsync(CreateSource createSource);
    Task<IReadOnlyCollection<Source>> GetAllAsync(CancellationToken cancellationToken);
    Task<Source?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}