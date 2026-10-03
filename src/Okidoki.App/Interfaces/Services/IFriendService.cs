using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Services;

public interface IFriendService
{
    Task<List<User>> GetFriendsAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<FriendRequest>> GetReceivedRequestsAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<FriendRequest>> GetSentRequestsAsync(long userId, CancellationToken cancellationToken = default);
    Task<FriendRequest> SendRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default);
    Task<FriendRequest> SendRequestAsync(long senderId, string username, CancellationToken cancellationToken = default);
    Task RevokeRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default);
    Task AcceptRequestAsync(long userId, long senderId, CancellationToken cancellationToken = default);
    Task DeclineRequestAsync(long userId, long senderId, CancellationToken cancellationToken = default);
}
