using System.ComponentModel.DataAnnotations.Schema;

namespace Okidoki.Domain.Entities;

public class GuildProfile
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }

    [NotMapped]
    public long? MainChannelId { get; set; }
}

public class Guild
{
    public long Id { get; set; }
    public long GuildProfileId { get; set; }
    public GuildProfile GuildProfile { get; set; } = null!;
    public ICollection<GuildChannelGroup> ChannelGroups { get; set; } = null!;
    public ICollection<GuildChannel> Channels { get; set; } = null!;
    public long? MainChannelId { get; set; }
    public ICollection<GuildRole> Roles { get; set; } = null!;
    public ICollection<GuildInvitation> Invitations { get; set; } = null!;
    public long OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public List<GuildMembership> GuildUsers { get; set; } = null!;
    public ICollection<GuildEventLog> EventLogs { get; set; } = null!;
}

public enum GuildEventType
{
    ChannelAdded,
    ChannelRemoved,
    UserKicked,
    UserLeft,
}

public class GuildEventLog
{
    public long Id { get; set; }
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;
    public GuildEventType EventType { get; set; }
    public long? EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
}

public class GuildRole
{
    public const string EveryoneRoleName = "everyone";

    public long Id { get; set; }
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;
    public ICollection<GuildMembership> UsersMembership { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ulong Permissions { get; set; }
    public uint? Color { get; set; }
}

public class GuildMembership
{
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<GuildRole> Roles { get; set; } = null!;
}

public class GuildChannelGroup
{
    public long Id { get; set; }
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;
    public ICollection<GuildChannel> Channels { get; set; } = null!;
    public int OrderNumber { get; set; }
    public string Name { get; set; } = null!;
}

public class GuildChannel
{
    public long Id { get; set; }
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public long? ChannelGroupId { get; set; }
    public GuildChannelGroup? ChannelGroup { get; set; }

    public ChannelType Type { get; set; }
    public int OrderNumber { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = null!;
    public ChatMessage? LastMessage => Messages?.LastOrDefault();
}

public class GuildInvitation
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public long GuildId { get; set; }
    public Guild Guild { get; set; } = null!;
    public DateTime? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int Uses { get; set; }
    public long CreatedById { get; set; } = 0;
    public User CreatedBy { get; set; } = null!;
}

public enum ChannelType
{
    Text,
    Voice,
}
