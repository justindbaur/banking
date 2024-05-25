using Banking.Abstractions.Entities;
using Banking.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Services.Implementations;

public class UserStore : IUserStore<User>
{
    private readonly BankingContext _context;

    public UserStore(BankingContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None); // Mutation operation
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        var updates = await _context.Users
            .Where(u => u.Id == user.Id)
            .ExecuteDeleteAsync(CancellationToken.None); // Mutation operation

        if (updates != 1)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "no_user_found",
                Description = "No user found.",
            });
        }

        return IdentityResult.Success;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        return await _context.Users.FindAsync([id], cancellationToken);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => string.Equals(u.UserName, normalizedUserName, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

        return user;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.UserName.ToLowerInvariant());
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        // We don't store a normalized user name, should we?
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users.FindAsync([user.Id], CancellationToken.None);

        if (existingUser == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "no_user_found",
                Description = "No user found.",
            });
        }

        // TODO: Do update
        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync(CancellationToken.None);
        return IdentityResult.Success;
    }
}
