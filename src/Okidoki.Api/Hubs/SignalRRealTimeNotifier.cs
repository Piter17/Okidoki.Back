using Microsoft.AspNetCore.SignalR;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.App.Mapping;
using Okidoki.Domain.Entities;

namespace Okidoki.Api.Hubs;

public sealed class SignalRRealTimeNotifier(
    IHubContext<RealTimeHub, IRealTimeClient> hubContext,
    IOnlineCache onlineCache)
    : IRealTimeNotifier
{
    private IRealTimeClient GuildClient(long guildId)
    {
        var users = onlineCache.GetGuildUsers(guildId).Select(x => x.ToString());
        return hubContext.Clients.Users(users);
    }

    private IRealTimeClient ChannelClient(long channelId)
    {
        var users = onlineCache.GetChannelUsers(channelId).Select(x => x.ToString());
        return hubContext.Clients.Users(users);
    }

    // Client for users that are friends or are on the same guilds
    private IRealTimeClient KnownUsersClient(long userId)
    {
        var users = onlineCache.GetAllKnownUsers(userId).Select(x => x.ToString());
        return hubContext.Clients.Users(users);
    }

    private IRealTimeClient SingleUserClient(long userId)
    {
        var user = userId.ToString();
        return hubContext.Clients.Users(user);
    }

    public Task AddMessageAsync(
        long guildId,
        ChatMessage message,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId)
                .AddMessage(ChatMessageMapper.ToDto(message), cancellationToken);

    public Task EditMessageAsync(
        long guildId,
        ChatMessage message,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId)
                .EditMessage(ChatMessageMapper.ToDto(message));

    public Task DeleteMessageAsync(
        long guildId,
        long messageId,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId).DeleteMessage(messageId);

    public Task FriendRequestSentAsync(
        long targetUserId,
        User userProfile,
        CancellationToken cancellationToken = default)
            => SingleUserClient(targetUserId)
                .FriendRequest(UserProfileMapper.ToDto(userProfile));

    public Task JoinGuildAsync(
        long userId,
        Guild guild,
        CancellationToken cancellationToken = default)
            => SingleUserClient(userId).JoinGuild(GuildMapper.ToDto(guild));

    public Task LeaveGuildAsync(
        long userId,
        long guildId,
        CancellationToken cancellationToken = default)
            => SingleUserClient(userId).LeaveGuild(guildId);

    public Task AddChannelAsync(
        long guildId,
        GuildChannel channel,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId).AddedChannel(GuildMapper.ToDto(channel));

    public Task RemoveChannelAsync(
        long guildId,
        long channelId,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId).RemovedChannel(guildId, channelId);

    public Task TypingAsync(
        long channelId,
        long userId,
        DateTime dateTime,
        CancellationToken cancellationToken = default)
        => ChannelClient(channelId).Typing(channelId, userId, dateTime);

    public Task GuildProfileChangedAsync(
        long guildId,
        GuildProfile guildProfile,
        CancellationToken cancellationToken = default)
            => GuildClient(guildId)
                .UpdatedGuildProfile(GuildMapper.ToDto(guildProfile), cancellationToken);

    public Task UserProfileChangedAsync(
        User userProfile,
        CancellationToken cancellationToken = default)
            => KnownUsersClient(userProfile.Id)
                .UpdatedProfile(UserProfileMapper.ToDto(userProfile), cancellationToken);

    public Task UserOnlineStateChangedAsync(
        long userId,
        UserOnlineState state,
        CancellationToken cancellationToken = default)
            => KnownUsersClient(userId)
                .UserOnlineStateChanged(userId, state, cancellationToken);
}

