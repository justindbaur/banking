using Banking.Abstractions.Entities;

namespace Banking.Api.Repositories.Yaml;

public class YamlSourceRepository : ISourceRepository
{
    private readonly IYamlRepository<Source> _yamlRepository;


    public YamlSourceRepository(IYamlRepositoryProvider yamlRepositoryProvider)
    {
        _yamlRepository = yamlRepositoryProvider.Get<Source>("sources");
    }

    public Task<Guid> CreateAsync(CreateSource createSource)
    {
        var newSource = new Source
        {
            Id = Guid.NewGuid(),
            SourceTemplateId = createSource.SourceTemplateId,
            DisplayName = createSource.DisplayName,
            Config = createSource.Config,
            Enabled = true,
        };

        _yamlRepository.Mutate(
            (items, newSource) => items.Add(newSource), newSource);

        return Task.FromResult(newSource.Id);
    }

    public Task<IReadOnlyCollection<Source>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_yamlRepository.Get());
    }

    public Task<Source?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var source = _yamlRepository.Get()
            .FirstOrDefault(s => s.Id == id);

        return Task.FromResult(source);
    }
}