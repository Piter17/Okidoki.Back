namespace Okidoki.App.Contracts;

public sealed class ChatMessageDto
{
    public required long Id { get; set; }
    public required long GuildChannelId { get; set; }
    public required long SenderId { get; set; }
    public required string Content { get; set; } = string.Empty;
    public required DateTime SendTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
}
