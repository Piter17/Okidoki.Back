namespace Okidoki.App.Contracts;

public sealed class ReceivedRequestDto
{
    public required long SenderId { get; set; }
    public required UserProfileDto Sender { get; set; } = null!;
    public required DateTime CreatedAt { get; set; }
}
