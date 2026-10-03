using Microsoft.EntityFrameworkCore;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Services;

public class FriendService(
    IFriendRepository friendRepository,
    IUserRepository userRepository,
    IRealTimeNotifier realTimeNotifier) : IFriendService
{
    public async Task<List<User>> GetFriendsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var friends = friendRepository.FriendsForUser(userId);
        var users = await friends
            .Select(x => x.UserAId == userId ? x.UserB : x.UserA)
            .Distinct()
            .ToListAsync(cancellationToken);

        return users;
    }

    public Task<List<FriendRequest>> GetReceivedRequestsAsync(long userId, CancellationToken cancellationToken)
    {
        return friendRepository.GetReceivedRequestsAsync(userId, cancellationToken);
    }

    public Task<List<FriendRequest>> GetSentRequestsAsync(long userId, CancellationToken cancellationToken)
    {
        return friendRepository.GetSentRequestsAsync(userId, cancellationToken);
    }

    public async Task<FriendRequest> SendRequestAsync(long senderId, string username, CancellationToken cancellationToken = default)
    {
        var target = await userRepository.CurrentUser
            .Where(x => x.Username == username)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"User '{username}' not found.");

        return await SendRequestAsync(senderId, target.Id, cancellationToken);
    }

    public async Task<FriendRequest> SendRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default)
    {
        var existing = await friendRepository.GetRequestAsync(senderId, targetId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var request = new FriendRequest
        {
            SenderId = senderId,
            TargetId = targetId,
            State = InvitationState.Sent,
            CreatedAt = DateTime.UtcNow,
        };

        var sender = await userRepository.GetUserAsync(senderId, cancellationToken)
            ?? throw new InvalidOperationException($"User {senderId} not found.");

        var target = await userRepository.GetUserAsync(targetId, cancellationToken)
            ?? throw new InvalidOperationException($"User {targetId} not found.");

        request.Sender = sender;
        request.Target = target;

        await realTimeNotifier.FriendRequestSentAsync(
            targetId,
            sender,
            cancellationToken);

        return request;
    }

    public Task RevokeRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default)
    {
        return friendRepository.UpdateRequestStateAsync(senderId, targetId, InvitationState.Revoked, cancellationToken);
    }

    public async Task AcceptRequestAsync(long userId, long senderId, CancellationToken cancellationToken = default)
    {
        var request = await friendRepository.GetRequestAsync(senderId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Friend request not found.");

        await friendRepository.UpdateRequestStateAsync(senderId, userId, InvitationState.Accepted, cancellationToken);

        var relation = new FriendRelation
        {
            UserAId = Math.Min(userId, senderId),
            UserBId = Math.Max(userId, senderId),
            CreatedAt = DateTime.UtcNow,
        };

        await friendRepository.AddRelationAsync(relation, cancellationToken);
    }

    public Task DeclineRequestAsync(long userId, long senderId, CancellationToken cancellationToken = default)
    {
        return friendRepository.UpdateRequestStateAsync(senderId, userId, InvitationState.Declined, cancellationToken);
    }
}
