using Microsoft.AspNetCore.Identity;

namespace Okidoki.Api.Configuration;

public class AnonymousRegisterOptions
{
    public AnonymousRegister AnonymousRegister {get; init;}
}

public enum AnonymousRegister{
    Deny,
    AllowOnInvite,
    Allow
}

public class RegisterOptionsDto
{
    public required AnonymousRegister AnonymousRegister { get; init; }
    public required PasswordOptions Password {get; init;}
}