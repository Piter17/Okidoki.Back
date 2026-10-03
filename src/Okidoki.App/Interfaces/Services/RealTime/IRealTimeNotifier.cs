using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Services.RealTime;

public interface IRealTimeNotifier
{
    Task AddMessageAsync(
        long guildId,
        ChatMessage message,
        CancellationToken cancellationToken = default);

    Task EditMessageAsync(
        long guildId,
        ChatMessage message,
        CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(
        long guildId,
        long messageId,
        CancellationToken cancellationToken = default);

    Task FriendRequestSentAsync(
        long targetUserId,
        User userProfile,
        CancellationToken cancellationToken = default);

    Task JoinGuildAsync(
        long userId,
        Guild guild,
        CancellationToken cancellationToken = default);

    Task LeaveGuildAsync(
        long userId,
        long guildId,
        CancellationToken cancellationToken = default);

    Task AddChannelAsync(
        long guildId,
        GuildChannel channel,
        CancellationToken cancellationToken = default);

    Task RemoveChannelAsync(
        long guildId,
        long channelId,
        CancellationToken cancellationToken = default);

    Task TypingAsync(
        long channelId,
        long userId,
        DateTime dateTime,
        CancellationToken cancellationToken = default);

    Task GuildProfileChangedAsync(
        long guildId,
        GuildProfile guildProfile,
        CancellationToken cancellationToken = default);

    Task UserProfileChangedAsync(
        User userProfile, 
        CancellationToken cancellationToken = default);

    Task UserOnlineStateChangedAsync(
        long userId,
        UserOnlineState state,
        CancellationToken cancellationToken = default);
}
