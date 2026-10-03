using Okidoki.App.Contracts;

namespace Okidoki.Api.Hubs;

public interface IRealTimeClient
{
    Task AddMessage(ChatMessageDto message, CancellationToken cancellationToken);
    Task EditMessage(ChatMessageDto modified);
    Task DeleteMessage(long id);
    Task FriendRequest(UserProfileDto userProfile);
    Task JoinGuild(GuildDto guild);
    Task LeaveGuild(long guildId);
    Task AddedChannel(GuildChannelDto channel);
    Task RemovedChannel(long guildId, long channelId);
    Task Typing(long channelId, long userId, DateTime dateTime);
    Task UserOnlineStateChanged(long userId, UserOnlineState state, CancellationToken cancellationToken);
    Task UpdatedProfile(UserProfileDto profile, CancellationToken cancellationToken);
    Task UpdatedGuildProfile(GuildProfileDto profile, CancellationToken cancellationToken);
}