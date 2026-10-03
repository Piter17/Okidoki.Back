using Okidoki.Domain.Entities;

namespace Okidoki.App.Contracts;

public sealed class CreateGuildChannelRequest
{
    public long? GroupId { get; set; }
    public required  string Name { get; set; } = string.Empty;
    public required ChannelType Type { get; set; }
}
