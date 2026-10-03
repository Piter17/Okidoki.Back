namespace Okidoki.App.Contracts;

public sealed class GuildInvitationDto
{
    public required long Id { get; set; }
    public required GuildProfileDto Guild { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public UserProfileDto CreatedBy { get; set; } = null!;
}
