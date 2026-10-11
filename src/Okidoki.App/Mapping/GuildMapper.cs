using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Mapping;

public static class GuildMapper
{
    public static GuildDto ToDto(Guild entity)
    {
        return new GuildDto
        {
            Id = entity.Id,
            Name = entity.GuildProfile?.Name ?? string.Empty,
            Image = entity.GuildProfile?.Image,
            Channels = entity.Channels?.Select(ToDto).ToList() ?? [],
            Roles = entity.Roles?.Select(ToDto).ToList() ?? [],
            OwnerId = entity.OwnerId,
            MainChannelId = entity.MainChannelId,
            OwnerUserName = entity.Owner?.Username ?? string.Empty,
            MemberCount = entity.GuildUsers?.Count ?? 0,
        };
    }

    public static GuildInvitationDto ToDto(GuildInvitation entity)
    {
        return new GuildInvitationDto
        {
            Id = entity.Id,
            Guild = GuildMapper.ToDto(entity.Guild),
            Code = entity.Code,
            CreatedBy = UserProfileMapper.ToDto(entity.CreatedBy),
        };
    }

    public static GuildChannelDto ToDto(GuildChannel entity)
    {
        return new GuildChannelDto
        {
            Id = entity.Id,
            GuildId = entity.GuildId,
            ChannelGroupId = entity.ChannelGroupId,
            Name = entity.Name,
            Type = entity.Type,
            OrderNumber = entity.OrderNumber,
        };
    }

    public static GuildRoleDto ToDto(GuildRole entity)
    {
        return new GuildRoleDto
        {
            Id = entity.Id,
            GuildId = entity.GuildId,
            Name = entity.Name,
            Permissions = entity.Permissions,
            Color = entity.Color,
        };
    }

    public static GuildProfileDto ToDto(GuildProfile entity)
    {
        return new GuildProfileDto
        {
            Id = entity.Id,
            Name = entity.Name,
            MainChannelId = entity.MainChannelId,
            Image = entity.Image,
        };
    }
}
