using System.Text.Json;
using Banking.Abstractions.Entities;

namespace Banking.Api.Repositories;

public record CreateSource(string SourceId, string DisplayName, JsonDocument Config);

public interface ISourceRepository
{
    Task<Guid> CreateAsync(CreateSource createSource);
    Task<IReadOnlyCollection<Source>> GetAllAsync();
    Task<Source?> GetByIdAsync(Guid id);
}