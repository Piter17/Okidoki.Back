namespace Okidoki.App.Contracts;

public sealed class GuildRoleDto
{
    public required long Id { get; set; }
    public required long GuildId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public ulong Permissions { get; set; }
    public uint? Color { get; set; }
}
