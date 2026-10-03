namespace Okidoki.App.Contracts;

public sealed class CreateGuildInvitationRequest
{
    public DateTime? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
}
