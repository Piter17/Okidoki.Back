using Okidoki.App.Services;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Repositories;

public interface IGuildRepository
{
    IQueryable<Guild> GuildsForUser(long userId);
    Task<IReadOnlyList<long>> GetIdsForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUserListAsync(long guildId, CancellationToken cancellationToken = default);
    Task<HashSet<GuildCacheEntry>> GetCacheAsync(CancellationToken cancellationToken = default);
    Task<GuildCacheEntry> GetCacheAsync(long guildId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(long userId, long guildId, CancellationToken cancellationToken = default);
    Task<GuildMembership?> GetMembershipAsync(long userId, long guildId, CancellationToken cancellationToken = default);
    Task<Guild?> GetByIdAsync(long guildId, CancellationToken cancellationToken = default);
    Task<List<GuildProfile>> GetForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task AddAsync(Guild guild, CancellationToken cancellationToken = default);
    Task<GuildInvitation?> GetInvitationAsync(string code, CancellationToken cancellationToken = default);
    Task<GuildInvitation?> GetInvitationFullAsync(string code, CancellationToken cancellationToken = default);
    Task IncrementInvitationUsesAsync(long invitationId, CancellationToken cancellationToken = default);
    Task AddInvitationAsync(GuildInvitation invitation, CancellationToken cancellationToken = default);
    Task AddChannelAsync(GuildChannel channel, GuildEventLog eventLog, CancellationToken cancellationToken = default);
    Task<bool> RemoveChannelAsync(long guildId, long channelId, GuildEventLog eventLog, CancellationToken cancellationToken = default);
    Task AddRoleAsync(GuildRole role, CancellationToken cancellationToken = default);
    Task AddMembershipAsync(GuildMembership membership, CancellationToken cancellationToken = default);
    Task<bool> RemoveMembershipAsync(long guildId, long userId, GuildEventLog eventLog, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(long roleId, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<bool> UnassignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<GuildProfile?> GetProfileAsync(long guildId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(GuildProfile profile, CancellationToken cancellationToken = default);
}
