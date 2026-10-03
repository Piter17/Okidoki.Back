namespace Okidoki.App.Contracts;

public sealed class FriendDto
{
    public required long UserId { get; set; }
    public required string UserName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public DateTime CreatedAt { get; set; }
}
