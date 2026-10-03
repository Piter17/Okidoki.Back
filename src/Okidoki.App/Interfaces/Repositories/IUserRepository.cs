using Okidoki.App.Services;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Repositories;

public interface IUserRepository
{
    IQueryable<User> CurrentUser { get; }
    Task<HashSet<UserCacheEntry>> GetCacheAsync(CancellationToken cancellationToken = default);
    Task<UserCacheEntry> GetCacheAsync(long userId, CancellationToken cancellationToken = default);
    Task<string> GetFreeUserNameAsync(string profileName);
    Task<bool> IsUserNameUsedAsync(string profileName);
    Task<User?> GetUserAsync(long id, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
}
