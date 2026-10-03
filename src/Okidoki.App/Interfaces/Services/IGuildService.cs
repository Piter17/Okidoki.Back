using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Services;

public interface IGuildService
{
    Task<Guild?> GetGuildAsync(long guildId, long currentUserId, CancellationToken cancellationToken = default);
    Task<List<GuildProfile>> GetGuildsAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersAsync(long actorId, long guildId, CancellationToken cancellationToken = default);
    Task<Guild> CreateGuildAsync(
        long ownerId,
        string name,
        Stream? image = null,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default);
    Task<GuildInvitation> CreateInvitationAsync(long guildId, long createdById, DateTime? validUntil, int? maxUses, CancellationToken cancellationToken = default);
    Task<GuildInvitation?> GetInvitationAsync(string invitationCode, CancellationToken cancellationToken = default);
    Task<Guild?> AcceptInvitationAsync(long userId, string invitationCode, CancellationToken cancellationToken = default);
    Task<bool> LeaveGuildAsync(long userId, long guildId, CancellationToken cancellationToken = default);
    Task<bool> KickUserAsync(long actorId, long guildId, long userId, CancellationToken cancellationToken = default);
    Task<GuildChannel> CreateChannelAsync(long userId, long guildId, long? groupId, string name, ChannelType type, CancellationToken cancellationToken = default);
    Task<bool> RemoveChannelAsync(long userId, long guildId, long channelId, CancellationToken cancellationToken = default);
    Task<GuildRole> CreateRoleAsync(long guildId, string name, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(long roleId, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<bool> UnassignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<GuildProfile?> UpdateImageAsync(long guildId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);
    Task<GuildProfile?> UpdateNameAsync(long guildId, string name, CancellationToken cancellationToken = default);
}
