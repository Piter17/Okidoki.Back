using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Repositories;

public interface IFriendRepository
{
    IQueryable<FriendRequest> RequestsForUser(long userId);
    IQueryable<FriendRelation> FriendsForUser(long userId);
    Task<FriendRequest?> GetRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default);
    Task<List<FriendRequest>> GetReceivedRequestsAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<FriendRequest>> GetSentRequestsAsync(long userId, CancellationToken cancellationToken = default);
    Task UpdateRequestStateAsync(long senderId, long targetId, InvitationState state, CancellationToken cancellationToken = default);
    Task AddRelationAsync(FriendRelation relation, CancellationToken cancellationToken = default);
}
