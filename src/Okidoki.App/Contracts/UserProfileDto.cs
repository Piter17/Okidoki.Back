namespace Okidoki.App.Contracts;

public sealed class UserProfileDto
{
    public required long Id { get; set; }
    public required string UserName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public UserOnlineState? State { get; set; }

    [CdnResource]
    public string? ProfilePicture { get; set; }

}
