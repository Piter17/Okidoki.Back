namespace Okidoki.Domain.Entities;

public class ChatMessage
{
    public long Id { get; set; }
    public long GuildChannelId { get; set; }
    public long SenderId { get; set; }
    public DateTime SendTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public required string Content { get; set; }
}
