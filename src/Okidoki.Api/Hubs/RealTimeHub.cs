using Microsoft.AspNetCore.SignalR;
using Okidoki.Api.Extensions;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;

namespace Okidoki.Api.Hubs;

public class RealTimeHub(
    IUserPresence userPresence,
    IUserRepository userRepository,
    IOnlineCache onlineCache,
    IRealTimeNotifier realTimeNotifier) : Hub<IRealTimeClient>
{
    // public static string GuildGroup(long guildId) => $"guild:{guildId}";

    // public async Task JoinGuild(long guildId)
    // {
    //     if (!await guildRepository.IsMemberAsync(Context.UserId, guildId))
    //     {
    //         throw new HubException("User is not a member of this guild.");
    //     }

        // await Groups.AddToGroupAsync(Context.ConnectionId, GuildGroup(guildId));
    // }

    // public Task LeaveGuild(long guildId) =>
    //     Groups.RemoveFromGroupAsync(Context.ConnectionId, GuildGroup(guildId));


    // public Task AddFriend()

    public Task Typing(long channelId)
        => realTimeNotifier.TypingAsync(channelId, Context.UserId, DateTime.UtcNow);

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserId;
        var wasOffline = userPresence.GetConnections(userId).Count == 0;
        userPresence.Add(userId, Context.ConnectionId);

        var user = await userRepository.GetCacheAsync(userId);
        await onlineCache.InitUserAsync(user);

        if (wasOffline)
        {
            await realTimeNotifier.UserOnlineStateChangedAsync(userId, UserOnlineState.Online);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var wasOnline = userPresence.GetConnections(Context.UserId).Count > 0;
        userPresence.Remove(Context.UserId, Context.ConnectionId);

        if (wasOnline && userPresence.GetConnections(Context.UserId).Count == 0)
        {
            await realTimeNotifier.UserOnlineStateChangedAsync(Context.UserId, UserOnlineState.Offline);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
