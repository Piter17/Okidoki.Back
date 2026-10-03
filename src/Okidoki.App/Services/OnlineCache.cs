using System.Collections.Concurrent;
using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.Domain.Authorization;

namespace Okidoki.App.Services;

public interface IUserCacheEntry<out TSet> where TSet : IReadOnlySet<long>
{
    long UserId { get; }
    string UserName { get; }
    TSet Guilds { get; }
    TSet Friends { get; }
}

public interface IGuildCacheEntry<out TSet> where TSet : IReadOnlySet<long>
{
    long GuildId { get; }
    TSet Users { get; }
    TSet Roles { get; }
    TSet Channels { get; }
}

public record UserCacheEntry(
    long UserId,
    string UserName,
    HashSet<long> Guilds,
    HashSet<long> Friends
): IUserCacheEntry<HashSet<long>>;
public record GuildCacheEntry(
    long GuildId,
    HashSet<long> Users,
    HashSet<long> Roles,
    HashSet<long> Channels
): IGuildCacheEntry<HashSet<long>>;

public sealed record UserCacheDumpEntry(
    long UserId,
    string UserName,
    IReadOnlySet<long> Guilds,
    IReadOnlySet<long> Friends
): IUserCacheEntry<IReadOnlySet<long>>;

public sealed record GuildCacheDumpEntry(
    long GuildId,
    IReadOnlySet<long> Users,
    IReadOnlySet<long> Roles,
    IReadOnlySet<long> Channels
): IGuildCacheEntry<IReadOnlySet<long>>;

public sealed class OnlineCacheDump : ReadonlyOnlineCache
{
    public OnlineCacheDump(
        IReadOnlyDictionary<long, UserCacheDumpEntry> users,
        IReadOnlyDictionary<long, GuildCacheDumpEntry> guilds,
        IReadOnlyDictionary<string, long> usersByName,
        IReadOnlyDictionary<long, IReadOnlySet<long>> usersByGuild,
        IReadOnlyDictionary<long, long> guildsByChannels)
    {
        Users = users;
        Guilds = guilds;
        UsersByName = usersByName;
        UsersByGuild = usersByGuild;
        GuildsByChannels = guildsByChannels;

        foreach (var (userId, user) in users)
        {
            _users[userId] = new UserCacheEntry(
                user.UserId,
                user.UserName,
                [.. user.Guilds],
                [.. user.Friends]);
        }

        foreach (var (guildId, guild) in guilds)
        {
            _guilds[guildId] = new GuildCacheEntry(
                guild.GuildId,
                [.. guild.Users],
                [.. guild.Roles],
                [.. guild.Channels]);
        }

        foreach (var (guildId, userIds) in usersByGuild)
            _usersByGuild[guildId] = [.. userIds];

        foreach (var (channelId, guildId) in guildsByChannels)
            _guildsByChannels[channelId] = guildId;
    }

    public IReadOnlyDictionary<long, UserCacheDumpEntry> Users { get; }
    public IReadOnlyDictionary<long, GuildCacheDumpEntry> Guilds { get; }
    public IReadOnlyDictionary<string, long> UsersByName { get; }
    public IReadOnlyDictionary<long, IReadOnlySet<long>> UsersByGuild { get; }
    public IReadOnlyDictionary<long, long> GuildsByChannels { get; }
}

public class ReadonlyOnlineCache : IReadOnlineCache
{
    protected readonly ConcurrentDictionary<long, UserCacheEntry> _users = new();
    protected readonly ConcurrentDictionary<long, GuildCacheEntry> _guilds = new();

    // indeksy
    protected readonly ConcurrentDictionary<string, long> _usersByName = new();
    protected readonly ConcurrentDictionary<long, HashSet<long>> _usersByGuild = new();
    protected readonly ConcurrentDictionary<long, long> _guildsByChannels = new();

    protected readonly Lock _lock = new(); // do aktualizacji indeksów

    public IReadOnlySet<long> GetAllKnownUsers(long userId)
    {
        var user = _users[userId];
        var result = new HashSet<long>(user.Friends)
        {
            userId
        };

        foreach (var guildId in user.Guilds)
        {
            if (_guilds.TryGetValue(guildId, out var guild))
            {
                foreach (var uid in guild.Users)
                    result.Add(uid);
            }
        }

        return result;
    }

    public IReadOnlySet<long> GetGuildUsers(long guildId)
    {
        lock (_lock)
        {
            return _guilds.TryGetValue(guildId, out var guild)
                ? guild.Users.ToHashSet()
                : [];
        }
    }
    
    public IReadOnlySet<long> GetChannelUsers(long channelId)
    {
        lock (_lock)
        {
            return _guildsByChannels.TryGetValue(channelId, out var guildId)
                ? _usersByGuild.TryGetValue(guildId, out var users)
                    ? users.ToHashSet()
                    : []
                : [];
        }
    }


    public IReadOnlySet<long> GetUserGuilds(long userId)
    {
        lock (_lock)
        {
            return _users.TryGetValue(userId, out var user)
                ? user.Guilds.ToHashSet()
                : [];
        }
    }
}

public partial class OnlineCache(
    IUserPresence _userPresence,
    ILogger<OnlineCache> _logger)
     : ReadonlyOnlineCache, IOnlineCache
{
    public OnlineCacheDump Dump()
    {
        lock (_lock)
        {
            return new(
                users: _users.ToFrozenDictionary(
                    pair => pair.Key,
                    pair => new UserCacheDumpEntry(
                        pair.Value.UserId,
                        pair.Value.UserName,
                        pair.Value.Guilds.ToFrozenSet(),
                        pair.Value.Friends.ToFrozenSet())),
                guilds: _guilds.ToFrozenDictionary(
                    pair => pair.Key,
                    pair => new GuildCacheDumpEntry(
                        pair.Value.GuildId,
                        pair.Value.Users.ToFrozenSet(),
                        pair.Value.Roles.ToFrozenSet(),
                        pair.Value.Channels.ToFrozenSet())),
                usersByName: _usersByName.ToFrozenDictionary(),
                usersByGuild: _usersByGuild.ToFrozenDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlySet<long>)pair.Value.ToFrozenSet()),
                guildsByChannels: _guildsByChannels.ToFrozenDictionary()
            );
        }
    }


    public async Task InitUserAsync(UserCacheEntry user)
    {
        lock (_lock)
        {
            if (_users.TryGetValue(user.UserId, out var previous))
            {
                _usersByName.TryRemove(previous.UserName, out _);
                foreach (var guildId in previous.Guilds)
                    RemoveUserFromGuildIndex(guildId, user.UserId);
            }

            _users[user.UserId] = user;
            _usersByName[user.UserName] = user.UserId;

            foreach (var guildId in user.Guilds)
                AddUserToGuildIndex(guildId, user.UserId);
        }
    }

    public async Task InitGuildAsync(GuildCacheEntry guild)
    {
        lock (_lock)
        {
            RemoveGuildIndexes(guild.GuildId);
            _guilds[guild.GuildId] = guild;

            foreach (var userId in guild.Users)
                AddUserToGuildIndex(guild.GuildId, userId);

            foreach (var channelId in guild.Channels)
                _guildsByChannels[channelId] = guild.GuildId;
        }
    }

    public IReadOnlySet<string> GetAllListenersForGuild(long guildId)
    {
        if (!_guilds.TryGetValue(guildId, out var guild))
            return new HashSet<string>();

        var result = new HashSet<string>();

        foreach (var user in guild.Users)
        {
            var connections = _userPresence.GetConnections(user);
            if (connections is not null)
            {
                foreach (var connection in connections)
                    result.Add(connection);
            }
        }

        return result;
    }

    public IReadOnlySet<string> GetAllListenersForUser(long userId)
    {
        var result = new HashSet<string>();
        var connections = _userPresence.GetConnections(userId);
        if (connections is not null)
        {
            foreach (var connection in connections)
                result.Add(connection);
        }

        return result;
    }

    public void AddToGuild(long guildId, long userId)
    {
        lock (_lock)
        {
            if (_guilds.TryGetValue(guildId, out var guild))
                guild.Users.Add(userId);

            if (_users.TryGetValue(userId, out var user))
                user.Guilds.Add(guildId);

            AddUserToGuildIndex(guildId, userId);
        }
    }

    public void RemoveFromGuild(long guildId, long userId)
    {
        lock (_lock)
        {
            if (_guilds.TryGetValue(guildId, out var guild))
                guild.Users.Remove(userId);

            if (_users.TryGetValue(userId, out var user))
                user.Guilds.Remove(guildId);

            RemoveUserFromGuildIndex(guildId, userId);
        }
    }

    public void AddChannel(long guildId, long channelId)
    {
        lock (_lock)
        {
            if (_guilds.TryGetValue(guildId, out var guild))
                guild.Channels.Add(channelId);

            _guildsByChannels[channelId] = guildId;
        }
    }

    public void RemoveChannel(long guildId, long channelId)
    {
        lock (_lock)
        {
            if (_guilds.TryGetValue(guildId, out var guild))
                guild.Channels.Remove(channelId);

            if (_guildsByChannels.TryGetValue(channelId, out var indexedGuildId)
                && indexedGuildId == guildId)
            {
                _guildsByChannels.TryRemove(channelId, out _);
            }
        }
    }

    public async Task FullInitAsync(
        HashSet<UserCacheEntry> users,
        HashSet<GuildCacheEntry> guilds,
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _users.Clear();
            _guilds.Clear();
            _usersByName.Clear();
            _usersByGuild.Clear();
            _guildsByChannels.Clear();

            foreach (var user in users)
            {
                _users[user.UserId] = user;
                _usersByName[user.UserName] = user.UserId;
                foreach (var guildId in user.Guilds)
                    AddUserToGuildIndex(guildId, user.UserId);
            }

            foreach (var guild in guilds)
            {
                _guilds[guild.GuildId] = guild;

                foreach (var userId in guild.Users)
                    AddUserToGuildIndex(guild.GuildId, userId);

                foreach (var channelId in guild.Channels)
                    _guildsByChannels[channelId] = guild.GuildId;
            }
        }

        LogInitialized(_users.Count, _guilds.Count);
    }

    private void AddUserToGuildIndex(long guildId, long userId)
    {
        if (!_usersByGuild.TryGetValue(guildId, out var users))
        {
            _usersByGuild[guildId] = [userId];
            return;
        }

        users.Add(userId);
    }

    private void RemoveUserFromGuildIndex(long guildId, long userId)
    {
        if (!_usersByGuild.TryGetValue(guildId, out var users))
            return;

        users.Remove(userId);
        if (users.Count == 0)
            _usersByGuild.TryRemove(guildId, out _);
    }

    private void RemoveGuildIndexes(long guildId)
    {
        _usersByGuild.TryRemove(guildId, out _);

        foreach (var channel in _guildsByChannels)
        {
            if (channel.Value == guildId)
                _guildsByChannels.TryRemove(channel.Key, out _);
        }
    }


    [LoggerMessage(Level = LogLevel.Information, Message = "Cache initialized with {userCount} users and {guildCount} guilds")]
    private partial void LogInitialized(int userCount, int guildCount);
}
