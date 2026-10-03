using Okidoki.Domain.Entities;

namespace Okidoki.App.Contracts;

public sealed class GuildChannelDto
{
    public required long Id { get; set; }
    public required long GuildId { get; set; }
    public long? ChannelGroupId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public ChannelType Type { get; set; }
    public int OrderNumber { get; set; }
}
