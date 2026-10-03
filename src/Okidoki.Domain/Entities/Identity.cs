using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Okidoki.Domain.Entities;

public class Identity : IdentityUser<long>
{
    public User User { get; set; } = null!;
}

public class ApplicationRole : IdentityRole<long>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName)
    {
        Name = roleName;
    }
}

public class User
{
    public long Id { get; set; }
    public required string Username { get; set; } = null!;
    public string? Nickname { get; set; }
    public string? ProfilePicture { get; set; }

    public ICollection<FriendRequest> FriendRequests { get; set; } = null!;
    public ICollection<FriendRequest> FriendInvitations { get; set; } = null!;
    [NotMapped]
    public ICollection<User> Friends { get; set; } = null!;
    public ICollection<FriendRelation> FriendsAsUserA { get; set; } = null!;
    public ICollection<FriendRelation> FriendsAsUserB { get; set; } = null!;
    [NotMapped]
    public ICollection<Guild> Guilds { get; set; } = null!;
    public ICollection<Guild> OwnedGuilds { get; set; } = null!;
    public List<GuildMembership> GuildMemberships { get; set; } = null!;
}

public abstract class SettingsBase
{
    public string Theme { get; set; } = "light";
    public uint PrimaryColor { get; set; } = 0xff0d6efd;
    public string Language { get; set; } = "pl";
}

public class UserSettings : SettingsBase
{
    public bool NotificationsEnabled { get; set; } = true;
}
