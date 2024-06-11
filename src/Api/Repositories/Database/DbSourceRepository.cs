using Banking.Abstractions.Entities;
using Banking.Storage;
using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Repositories.Database;

public class DbSourceRepository : ISourceRepository
{
    private readonly BankingContext _bankingContext;


    public DbSourceRepository(BankingContext bankingContext)
    {
        _bankingContext = bankingContext;
    }

    public async Task<Guid> CreateAsync(CreateSource createSource)
    {
        var newSource = new Source
        {
            SourceTemplateId = createSource.SourceId,
            Config = createSource.Config,
            Enabled = true,
            DisplayName = null,
        };

        _bankingContext.Sources.Add(newSource);

        await _bankingContext.SaveChangesAsync();
        return newSource.Id;
    }

    public async Task<IReadOnlyCollection<Source>> GetAllAsync()
    {
        return await _bankingContext.Sources.ToListAsync();
    }

    public async Task<Source?> GetByIdAsync(Guid id)
    {
        return await _bankingContext.Sources.FindAsync(id);
    }
}