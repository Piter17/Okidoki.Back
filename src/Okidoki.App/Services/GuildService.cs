using IdGen;
using Okidoki.App.Extensions;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.App.Mapping;
using Okidoki.Domain.Authorization;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Services;

public class GuildService(
    IGuildRepository guildRepository,
    IUserRepository userRepository,
    IIdGenerator<long> idGenerator,
    IImageStorageService imageStorage,
    ISecureStringGenerator secureStringGenerator,
    IOnlineCache onlineCache,
    IRealTimeNotifier realTimeNotifier,
    IGuildAuthorizationService authorization
    ) : IGuildService
{
    public async Task<Guild?> GetGuildAsync(long guildId, long currentUserId, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken);
        if (guild is null)
        {
            return null;
        }

        var isMember = guild.GuildUsers.Any(x => x.UserId == currentUserId);
        return isMember ? guild : null;
    }

    public Task<List<GuildProfile>> GetGuildsAsync(long userId, CancellationToken cancellationToken = default)
    {
        return guildRepository.GetForUserAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(long actorId, long guildId, CancellationToken cancellationToken = default)
    {
        await authorization.EnsurePermissionAsync(actorId, guildId, Permissions.ReadContent, cancellationToken);
        return await guildRepository.GetUserListAsync(guildId, cancellationToken);
    }

    public async Task<Guild> CreateGuildAsync(
        long ownerId,
        string name,
        Stream? image = null,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var owner = await userRepository.GetUserAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException($"User {ownerId} not found.");

        var guildId = idGenerator.CreateId();
        string? imageResourceUri = null;

        if (image is not null)
        {
            var upload = await imageStorage.UploadGuildImageAsync(
                guildId,
                image,
                fileName ?? "image",
                contentType,
                cancellationToken);

            if (!upload.Success || upload.ResourceUri is null)
            {
                throw new InvalidOperationException("Guild image upload failed.", upload.Error);
            }

            imageResourceUri = upload.ResourceUri.ToString();
        }

        try
        {
            var mainChannelId = idGenerator.CreateId();

            var everyoneRole = new GuildRole
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                Name = GuildRole.EveryoneRoleName,
                Permissions = (ulong)(Permissions.ReadContent | Permissions.SendMessages | Permissions.SendAttachments),
                Color = 0x50ffffff,
            };

            var guildProfile = new GuildProfile
            {
                Id = guildId,
                Name = name,
                Image = imageResourceUri,
            };

            var mainChannel = new GuildChannel
            {
                Id = mainChannelId,
                GuildId = guildId,
                Name = "Tekstowy",
                OrderNumber = 0,
                Type = ChannelType.Text,
                Messages = [],
            };

            var voiceChannel = new GuildChannel
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                Name = "Głosowy",
                OrderNumber = 1,
                Type = ChannelType.Voice,
                Messages = [],
            };

            var guild = new Guild
            {
                Id = guildId,
                GuildProfileId = guildId,
                GuildProfile = guildProfile,
                MainChannelId = mainChannelId,
                OwnerId = ownerId,
                Owner = owner,
                Roles = [everyoneRole],
                Channels = [mainChannel, voiceChannel],
                Invitations = [],
                GuildUsers = [],
                ChannelGroups = [],
            };

            var member = new GuildMembership
            {
                GuildId = guildId,
                Guild = guild,
                UserId = ownerId,
                User = owner,
                Roles = [everyoneRole],
            };

            guild.GuildUsers.Add(member);
            owner.GuildMemberships ??= [];
            owner.GuildMemberships.Add(member);

            await guildRepository.AddAsync(guild, cancellationToken);
            await onlineCache.InitGuildAsync(new GuildCacheEntry(
                guild.Id,
                [.. guild.GuildUsers.Select(x => x.UserId)],
                [.. guild.Roles.Select(x => x.Id)],
                [.. guild.Channels.Select(x => x.Id)]
            ));
            onlineCache.AddToGuild(guild.Id, ownerId);
            await realTimeNotifier.JoinGuildAsync(ownerId, guild, cancellationToken);

            return guild;
        }
        catch
        {
            if (imageResourceUri is not null)
            {
                try
                {
                    await imageStorage.DeleteAsync(imageResourceUri, CancellationToken.None);
                }
                catch
                {
                    // The original persistence error is more useful to the caller.
                }
            }

            throw;
        }
    }

    public async Task<GuildInvitation> CreateInvitationAsync(long guildId, long createdById, DateTime? validUntil, int? maxUses, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken)
            ?? throw new InvalidOperationException($"Guild {guildId} not found.");

        var invitation = new GuildInvitation
        {
            Id = idGenerator.CreateId(),
            GuildId = guildId,
            Guild = guild,
            CreatedById = createdById,
            ValidUntil = validUntil,
            MaxUses = maxUses,
            Code = secureStringGenerator.GenerateInvitationCode(),
            CreatedBy = await userRepository.GetUserAsync(createdById, cancellationToken)
                ?? throw new InvalidOperationException($"User {createdById} not found."),
        };

        guild.Invitations ??= [];
        guild.Invitations.Add(invitation);
        await guildRepository.AddInvitationAsync(invitation, cancellationToken);

        return invitation;
    }

    public Task<GuildInvitation?> GetInvitationAsync(string invitationCode, CancellationToken cancellationToken = default)
    {
        return guildRepository.GetInvitationAsync(invitationCode, cancellationToken);
    }

    public async Task<Guild?> AcceptInvitationAsync(long userId, string invitationCode, CancellationToken cancellationToken = default)
    {
        var invitation = await guildRepository.GetInvitationAsync(invitationCode, cancellationToken);
        if (invitation?.Guild is null)
        {
            return null;
        }

        if (invitation.Guild.GuildUsers.Any(x => x.UserId == userId))
        {
            return invitation.Guild;
        }

        await guildRepository.AddMembershipAsync(new GuildMembership
        {
            GuildId = invitation.GuildId,
            UserId = userId,
            Roles = [invitation.Guild.Roles.Everyone]
        }, cancellationToken);

        onlineCache.AddToGuild(invitation.GuildId, userId);
        var addedGuild = await guildRepository.GetByIdAsync(invitation.GuildId, cancellationToken);
        if (addedGuild is not null)
        {
            await realTimeNotifier.JoinGuildAsync(userId, addedGuild, cancellationToken);
        }

        await guildRepository.IncrementInvitationUsesAsync(invitation.Id);

        return await guildRepository.GetByIdAsync(invitation.GuildId, cancellationToken);
    }

    public async Task<bool> LeaveGuildAsync(long userId, long guildId, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken);
        if (guild is null || guild.OwnerId == userId)
        {
            return false;
        }

        var result = await guildRepository.RemoveMembershipAsync(
            guildId,
            userId,
            new GuildEventLog
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                EventType = GuildEventType.UserLeft,
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
            },
            cancellationToken);

        if (result)
        {
            onlineCache.RemoveFromGuild(guildId, userId);
            await realTimeNotifier.LeaveGuildAsync(userId, guildId, cancellationToken);
        }

        return result;
    }

    public async Task<bool> KickUserAsync(long actorId, long guildId, long userId, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken);
        if (guild is null || guild.OwnerId == userId || actorId == userId || !guild.GuildUsers.Any(x => x.UserId == actorId))
        {
            return false;
        }

        await authorization.EnsurePermissionAsync(actorId, guildId, Permissions.KickUser, cancellationToken);

        var result = await guildRepository.RemoveMembershipAsync(
            guildId,
            userId,
            new GuildEventLog
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                EventType = GuildEventType.UserKicked,
                EntityId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = actorId,
            },
            cancellationToken);

        if (result)
        {
            onlineCache.RemoveFromGuild(guildId, userId);
            await realTimeNotifier.LeaveGuildAsync(userId, guildId, cancellationToken);
        }

        return result;
    }

    public async Task<GuildChannel> CreateChannelAsync(long userId, long guildId, long? groupId, string name, ChannelType type, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken)
            ?? throw new InvalidOperationException($"Guild {guildId} not found.");
        await authorization.EnsurePermissionAsync(userId, guildId, Permissions.CreateChannels, cancellationToken);

        var channel = new GuildChannel
        {
            Id = idGenerator.CreateId(),
            GuildId = guildId,
            ChannelGroupId = groupId,
            Name = name,
            Type = type,
            OrderNumber = guild.Channels?.Count > 0 ? guild.Channels.Max(x => x.OrderNumber) + 1 : 0,
        };

        await guildRepository.AddChannelAsync(
            channel,
            new GuildEventLog
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                EventType = GuildEventType.ChannelAdded,
                EntityId = channel.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
            },
            cancellationToken);
        onlineCache.AddChannel(guildId, channel.Id);
        await realTimeNotifier.AddChannelAsync(guildId, channel, cancellationToken);
        return channel;
    }

    public async Task<bool> RemoveChannelAsync(long userId, long guildId, long channelId, CancellationToken cancellationToken = default)
    {
        var guild = await guildRepository.GetByIdAsync(guildId, cancellationToken);
        if (guild is null || !guild.GuildUsers.Any(x => x.UserId == userId))
        {
            return false;
        }

        await authorization.EnsurePermissionAsync(userId, guildId, Permissions.RemoveChannels, cancellationToken);

        var removed = await guildRepository.RemoveChannelAsync(
            guildId,
            channelId,
            new GuildEventLog
            {
                Id = idGenerator.CreateId(),
                GuildId = guildId,
                EventType = GuildEventType.ChannelRemoved,
                EntityId = channelId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
            },
            cancellationToken);

        if (removed)
        {
            onlineCache.RemoveChannel(guildId, channelId);
            await realTimeNotifier.RemoveChannelAsync(guildId, channelId, cancellationToken);
        }

        return removed;
    }

    public async Task<GuildRole> CreateRoleAsync(long guildId, string name, CancellationToken cancellationToken = default)
    {
        var role = new GuildRole
        {
            Id = idGenerator.CreateId(),
            GuildId = guildId,
            Name = name,
            Permissions = 0,
        };

        await guildRepository.AddRoleAsync(role, cancellationToken);
        return role;
    }

    public Task<bool> RemoveRoleAsync(long roleId, CancellationToken cancellationToken = default)
        => guildRepository.RemoveRoleAsync(roleId, cancellationToken);

    public Task<bool> AssignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
        => guildRepository.AssignRoleAsync(userId, roleId, cancellationToken);

    public Task<bool> UnassignRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
        => guildRepository.UnassignRoleAsync(userId, roleId, cancellationToken);

    public async Task<GuildProfile?> UpdateImageAsync(long guildId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var profile = await guildRepository.GetProfileAsync(guildId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var result = await imageStorage.UploadGuildImageAsync(guildId, content, fileName, contentType, cancellationToken);
        if (!result.Success || result.ResourceUri is null)
        {
            throw new InvalidOperationException("Guild image upload failed.", result.Error);
        }

        profile.Image = result.ResourceUri!.ToString();
        await guildRepository.UpdateProfileAsync(profile, cancellationToken);
        return profile;
    }

    public async Task<GuildProfile?> UpdateNameAsync(long guildId, string name, CancellationToken cancellationToken = default)
    {
        var profile = await guildRepository.GetProfileAsync(guildId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        profile.Name = name.Trim();
        await guildRepository.UpdateProfileAsync(profile, cancellationToken);
        return profile;
    }


}
