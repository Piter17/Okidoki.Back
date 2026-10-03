namespace Okidoki.App.Contracts;

public class AnonymusRegistrationRequest
{
    public required string ProfileName { get; set; } = null!;
    public required string? GuildInvitationCode { get; set; }
}

public sealed class RegistrationRequest : AnonymusRegistrationRequest
{
    public required string Email { get; set; } = null!;
    public required string Password { get; set; } = null!;
}

public sealed class AddCredentialsRequest
{
    public required string Email { get; set; } = null!;
    public required string Password { get; set; } = null!;
}

public sealed class RegisterResponse
{
    public required long UserId { get; init; }
    public required string UserName { get; init; } = null!;
}
