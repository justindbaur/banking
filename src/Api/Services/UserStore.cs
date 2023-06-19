using Banking.Abstractions.Entities;
using Banking.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Services;

public sealed class UserStore : IUserStore<User>
{
    private readonly BankingContext _bankingContext;

    public UserStore(BankingContext bankingContext)
    {
        _bankingContext = bankingContext;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        _bankingContext.Users.Add(user);
        await _bankingContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        var existingUser = await _bankingContext.Users.Where(u => u.Id == user.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingUser is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "not_found",
                Description = "Could not find the requested user",
            });
        }

        _bankingContext.Users.Remove(existingUser);

        await _bankingContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public void Dispose()
    {
        // nothing needed
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var actualUserId))
        {
            throw new InvalidOperationException("userId is not in a valid format");
        }

        return await FindByIdCoreAsync(actualUserId, cancellationToken);
    }

    private async Task<User?> FindByIdCoreAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _bankingContext.Users.Where(u => u.Id == userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult<User?>(null);
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.Username);
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(userName);

        user.Username = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        // Nothing to actually update, we only track an id
        var existingUser = await FindByIdCoreAsync(user.Id, cancellationToken);

        if (existingUser is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "not_found",
                Description = "User with requested id was not found.",
            });
        }

        existingUser.Username = user.Username;

        _bankingContext.Users.Update(existingUser);
        await _bankingContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }
}
