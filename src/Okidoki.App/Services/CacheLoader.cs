using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.App.Services;

public class CacheLoader(
    IUserRepository userRepository,
    IGuildRepository guildRepository,
    IOnlineCache cache) : ICacheLoader
{
    public async Task InitAsync(CancellationToken cancellationToken)
    {
        var users = await userRepository.GetCacheAsync(cancellationToken);
        var guilds = await guildRepository.GetCacheAsync(cancellationToken);

        await cache.FullInitAsync(users, guilds, cancellationToken);
    }

    public async Task InitUserAsync(long userId)
    {
        var user = await userRepository.GetCacheAsync(userId);
        await cache.InitUserAsync(user);
    }
}
