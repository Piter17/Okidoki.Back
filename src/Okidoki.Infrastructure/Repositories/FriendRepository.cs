using Microsoft.EntityFrameworkCore;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;

namespace Okidoki.Infrastructure.Repositories;

public class FriendRepository(AppDbContext context) : IFriendRepository
{
    public IQueryable<FriendRequest> RequestsForUser(long userId)
        => context.FriendRequests
            .Where(x => x.TargetId == userId || x.SenderId == userId)
            .AsNoTracking();

    public IQueryable<FriendRelation> FriendsForUser(long userId)
        => context.FriendRelations
            .Where(x => x.UserAId == userId || x.UserBId == userId)
            .AsNoTracking();

    public Task<FriendRequest?> GetRequestAsync(long senderId, long targetId, CancellationToken cancellationToken = default)
        => context.FriendRequests
            .FirstOrDefaultAsync(x => x.SenderId == senderId && x.TargetId == targetId, cancellationToken);

    public Task<List<FriendRequest>> GetReceivedRequestsAsync(long userId, CancellationToken cancellationToken = default)
        => context.FriendRequests
            .Where(x => x.TargetId == userId)
            .Include(x => x.Sender)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<List<FriendRequest>> GetSentRequestsAsync(long userId, CancellationToken cancellationToken = default)
        => context.FriendRequests
            .Where(x => x.SenderId == userId)
            .Include(x => x.Target)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task UpdateRequestStateAsync(long senderId, long targetId, InvitationState state, CancellationToken cancellationToken = default)
    {
        return context.FriendRequests
            .Where(x => x.SenderId == senderId && x.TargetId == targetId)
            .ExecuteUpdateAsync(x => x.SetProperty(request => request.State, state), cancellationToken);
    }

    public async Task AddRelationAsync(FriendRelation relation, CancellationToken cancellationToken = default)
    {
        await context.FriendRelations.AddAsync(relation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
