namespace Okidoki.App.Contracts;

public sealed class SentRequestDto
{
    public required long TargetId { get; set; }
    public required UserProfileDto Target { get; set; } = null!;
    public required DateTime CreatedAt { get; set; }
}
