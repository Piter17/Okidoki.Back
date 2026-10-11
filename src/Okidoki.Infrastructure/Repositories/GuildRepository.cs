using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Services;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;

namespace Okidoki.Infrastructure.Repositories;

public class GuildRepository(AppDbContext context) : IGuildRepository
{
    public IQueryable<Guild> GuildsForUser(long userId) =>
        context.GuildUsers
            .Where(x => x.UserId == userId)
            .Select(x => x.Guild);

    public async Task<IReadOnlyList<long>> GetIdsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await context.GuildUsers
            .Where(x => x.UserId == userId)
            .Select(x => x.GuildId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUserListAsync(long guildId, CancellationToken cancellationToken = default)
    {
        var entities = await context.GuildUsers
            .Where(x => x.GuildId == guildId)
            .Include(x => x.User)
            .Select(x => x.User)
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        return entities;
    }
    
    private IQueryable<GuildCacheEntry> GuildCache
        => context.Guilds
            .Include(x => x.GuildUsers)
            .Include(x => x.Roles)
            .Select(x => new GuildCacheEntry(
                x.Id,
                x.GuildUsers.Select(m => m.UserId).ToHashSet(),
                x.Roles.Select(g => g.Id).ToHashSet(),
                x.Channels.Select(channel => channel.Id).ToHashSet()
            ));

    public Task<HashSet<GuildCacheEntry>> GetCacheAsync(
        CancellationToken cancellationToken = default)
        => GuildCache
            .ToHashSetAsync(cancellationToken);

    public Task<GuildCacheEntry> GetCacheAsync(
        long guildId,
        CancellationToken cancellationToken = default)
        => GuildCache
            .Where(x => x.GuildId == guildId)
            .SingleAsync(cancellationToken);

    public Task<bool> IsMemberAsync(long userId, long guildId, CancellationToken cancellationToken = default)
        => context.GuildUsers.AnyAsync(
            membership => membership.UserId == userId && membership.GuildId == guildId,
            cancellationToken);

    public Task<GuildMembership?> GetMembershipAsync(long userId, long guildId, CancellationToken cancellationToken = default)
        => context.GuildUsers
            .Include(x => x.Roles)
            .Include(x => x.Guild)
            .FirstOrDefaultAsync(
                membership => membership.UserId == userId && membership.GuildId == guildId,
                cancellationToken);

    public Task<Guild?> GetByIdAsync(long guildId, CancellationToken cancellationToken = default)
        => context.Guilds
            .Include(x => x.GuildProfile)
            .Include(x => x.Roles)
            .Include(x => x.Channels)
            .Include(x => x.GuildUsers)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == guildId, cancellationToken);

    public async Task<List<GuildProfile>> GetForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var profiles = await context.GuildUsers
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                Profile = x.Guild.GuildProfile,
                x.Guild.MainChannelId,
                GuildId = x.Guild.Id,
            })
            .OrderByDescending(x => x.GuildId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var item in profiles)
        {
            item.Profile.MainChannelId = item.MainChannelId;
        }

        return profiles.ConvertAll(x => x.Profile);
    }

    public async Task AddAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        await context.Guilds.AddAsync(guild, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<GuildInvitation?> GetInvitationAsync(string code, CancellationToken cancellationToken = default)
        => context.GuildInvitations
            .Include(x => x.Guild.GuildProfile)
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Code == code && (x.ValidUntil == null || x.ValidUntil > DateTime.UtcNow), cancellationToken);

        public Task<GuildInvitation?> GetInvitationFullAsync(string code, CancellationToken cancellationToken = default)
        => context.GuildInvitations
            .Include(x => x.Guild.GuildProfile)
            .Include(x => x.Guild.GuildUsers)
            .Include(x => x.Guild.Roles)
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Code == code && (x.ValidUntil == null || x.ValidUntil > DateTime.UtcNow), cancellationToken);

    public Task IncrementInvitationUsesAsync(long invitationId, CancellationToken cancellationToken = default)
        => context.GuildInvitations
            .Where(x => x.Id == invitationId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.Uses, x => x.Uses + 1),
                cancellationToken: cancellationToken);

    public async Task AddInvitationAsync(GuildInvitation invitation, CancellationToken cancellationToken = default)
    {
        await context.GuildInvitations.AddAsync(invitation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddChannelAsync(GuildChannel channel, GuildEventLog eventLog, CancellationToken cancellationToken = default)
    {
        await context.GuildChannels.AddAsync(channel, cancellationToken);
        await context.GuildEventLogs.AddAsync(eventLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveChannelAsync(long guildId, long channelId, GuildEventLog eventLog, CancellationToken cancellationToken = default)
    {
        var channel = await context.GuildChannels
            .FirstOrDefaultAsync(x => x.Id == channelId && x.GuildId == guildId, cancellationToken);
        if (channel is null)
        {
            return false;
        }

        context.GuildChannels.Remove(channel);
        await context.GuildEventLogs.AddAsync(eventLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task AddRoleAsync(GuildRole role, CancellationToken cancellationToken = default)
    {
        await context.GuildRoles.AddAsync(role, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMembershipAsync(GuildMembership membership, CancellationToken cancellationToken = default)
    {
        await context.GuildUsers.AddAsync(membership, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveMembershipAsync(long guildId, long userId, GuildEventLog eventLog, CancellationToken cancellationToken = default)
    {
        var membership = await context.GuildUsers
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId, cancellationToken);
        if (membership is null)
        {
            return false;
        }

        context.GuildUsers.Remove(membership);
        await context.GuildEventLogs.AddAsync(eventLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await context.GuildRoles.FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role is null)
        {
            return false;
        }

        context.GuildRoles.Remove(role);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
    {
        var role = await context.GuildRoles
            .Include(x => x.UsersMembership)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        var membership = role?.UsersMembership.FirstOrDefault(x => x.UserId == userId);
        if (membership is null)
        {
            return false;
        }

        membership.Roles ??= new List<GuildRole>();
        if (!membership.Roles.Any(x => x.Id == roleId))
        {
            membership.Roles.Add(role!);
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnassignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
    {
        var membership = await context.GuildUsers
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Roles.Any(role => role.Id == roleId), cancellationToken);
        var role = membership?.Roles.FirstOrDefault(x => x.Id == roleId);
        if (role is null)
        {
            return false;
        }

        membership!.Roles.Remove(role);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<GuildProfile?> GetProfileAsync(long guildId, CancellationToken cancellationToken = default)
        => context.GuildProfiles.FirstOrDefaultAsync(x => x.Id == guildId, cancellationToken);

    public async Task UpdateProfileAsync(GuildProfile profile, CancellationToken cancellationToken = default)
    {
        context.GuildProfiles.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }
}
