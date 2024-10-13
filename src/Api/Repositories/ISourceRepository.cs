using System.Text.Json;
using System.Text.Json.Nodes;
using Banking.Abstractions.Entities;

namespace Banking.Api.Repositories;

public record CreateSource(string SourceTemplateId, string DisplayName, JsonNode Config);

public interface ISourceRepository
{
    Task<Guid> CreateAsync(CreateSource createSource);
    Task<IReadOnlyCollection<Source>> GetAllAsync(CancellationToken cancellationToken);
    Task<Source?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}