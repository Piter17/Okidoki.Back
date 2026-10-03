namespace Okidoki.App.Contracts;

public sealed class IdentityDto
{
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public required long UserId { get; set; }
    public required bool IsEmailConfirmed { get; set; }
}
