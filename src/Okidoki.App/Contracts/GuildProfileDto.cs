namespace Okidoki.App.Contracts;

public class GuildProfileDto
{
    public required long Id { get; set; }
    public required string Name { get; set; } = null!;
    public long? MainChannelId {get; set; }

    [CdnResource]
    public string? Image { get; set; }
}
