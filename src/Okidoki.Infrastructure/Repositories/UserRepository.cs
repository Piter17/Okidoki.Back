using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Services;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;

namespace Okidoki.Infrastructure.Repositories;

public class UserRepository(AppDbContext context, ClaimsPrincipal claims, Random random) : IUserRepository
{
    public IQueryable<User> CurrentUser =>
        context.Accounts.Where(x => x.Id == GetCurrentUserId());

    private async Task<HashSet<UserCacheEntry>> LoadCacheAsync(
        long? userId,
        CancellationToken cancellationToken)
    {
        var usersQuery = context.Accounts
            .AsNoTracking()
            .Select(x => new { x.Id, x.Username });

        if (userId != null)
        {
            usersQuery = usersQuery.Where(x => x.Id == userId);
        }

        var users = await usersQuery.ToListAsync(cancellationToken);
        var userIds = users.Select(x => x.Id).ToArray();
        var memberships = await context.GuildUsers
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .Select(x => new { x.UserId, x.GuildId })
            .ToListAsync(cancellationToken);
        var relations = await context.FriendRelations
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserAId) || userIds.Contains(x.UserBId))
            .Select(x => new { x.UserAId, x.UserBId })
            .ToListAsync(cancellationToken);

        var guildsByUser = memberships
            .GroupBy(x => x.UserId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.GuildId).ToHashSet());
        var friendsByUser = userIds.ToDictionary(x => x, _ => new HashSet<long>());
        foreach (var relation in relations)
        {
            if (friendsByUser.TryGetValue(relation.UserAId, out var userAFriends))
                userAFriends.Add(relation.UserBId);
            if (friendsByUser.TryGetValue(relation.UserBId, out var userBFriends))
                userBFriends.Add(relation.UserAId);
        }

        return users
            .Select(x => new UserCacheEntry(
                x.Id,
                x.Username,
                guildsByUser.GetValueOrDefault(x.Id, []),
                friendsByUser[x.Id]))
            .ToHashSet();
    }

    public Task<HashSet<UserCacheEntry>> GetCacheAsync(
        CancellationToken cancellationToken = default)
        => LoadCacheAsync(null, cancellationToken);


    public async Task<UserCacheEntry> GetCacheAsync(
        long userId,
        CancellationToken cancellationToken = default)
        => (await LoadCacheAsync(userId, cancellationToken)).Single();

    public async Task<string> GetFreeUserNameAsync(string profileName)
    {
        var cmpString = profileName + "#";
        var totalLen = profileName.Length + 4;
        var used = await context.Accounts
            .Where(x => x.Username.StartsWith(profileName)
                && x.Username.Length == totalLen)
            .Select(x => x.Username)
            .AsNoTracking()
            .ToListAsync()!;

        string generated;
        int i = 0;

        do
        {
            generated = $"{profileName}#" + random.Next(9999).ToString().PadLeft(4, '0');
            i++;
            if (i > 100)
                return $"{profileName}#" + random.Next(99999999).ToString().PadLeft(8, '0');
        }
        while (used.Contains(generated));

        return generated;
    }

    public Task<bool> IsUserNameUsedAsync(string userName)
        => context.Accounts.AnyAsync(x => x.Username == userName);

    public async Task<User?> GetUserAsync(long id, CancellationToken cancellationToken = default)
        => await context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);


    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Accounts.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    private long GetCurrentUserId()
    {
        var value = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claims.FindFirst("sub")?.Value
            ?? "0";

        return long.TryParse(value, out var userId) ? userId : 0;
    }
}
