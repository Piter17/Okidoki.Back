namespace Okidoki.Domain.Entities;

public class FriendRelation
{
    public long UserAId { get; set; }
    public User UserA { get; set; } = null!;

    public long UserBId { get; set; }
    public User UserB { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

public class FriendRequest
{
    public long SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public long TargetId { get; set; }
    public User Target { get; set; } = null!;

    public InvitationState State { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum InvitationState
{
    Sent,
    Accepted,
    Declined,
    Revoked,
}
