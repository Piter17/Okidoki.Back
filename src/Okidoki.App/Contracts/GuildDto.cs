namespace Okidoki.App.Contracts;

public sealed class GuildDto : GuildProfileDto
{
    public long OwnerId { get; set; }
    public List<GuildRoleDto> Roles { get; set; } = null!;
    public List<GuildChannelDto> Channels { get; set; } = null!;
    public string OwnerUserName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}
