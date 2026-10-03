using Okidoki.App.Services;

namespace Okidoki.App.Interfaces.Services;

public interface IOnlineCache : IReadOnlineCache
{
    Task FullInitAsync(HashSet<UserCacheEntry> users, HashSet<GuildCacheEntry> guilds, CancellationToken cancellationToken);
    void AddToGuild(long guildId, long userId);
    void RemoveFromGuild(long guildId, long userId);
    void AddChannel(long guildId, long channelId);
    void RemoveChannel(long guildId, long channelId);
    Task InitUserAsync(UserCacheEntry user);
    Task InitGuildAsync(GuildCacheEntry guild);
    IReadOnlySet<string> GetAllListenersForGuild(long guildId);
    IReadOnlySet<string> GetAllListenersForUser(long userId);
    OnlineCacheDump Dump();
}

public interface IReadOnlineCache
{
    IReadOnlySet<long> GetUserGuilds(long userId);
    IReadOnlySet<long> GetGuildUsers(long guildId);
    IReadOnlySet<long> GetChannelUsers(long channelId);
    IReadOnlySet<long> GetAllKnownUsers(long userId);
}