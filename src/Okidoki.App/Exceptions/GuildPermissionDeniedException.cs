using Okidoki.Domain.Authorization;

namespace Okidoki.App.Exceptions;

public sealed class GuildPermissionDeniedException(Permissions permission)
    : Exception($"The user does not have the '{permission}' permission.")
{
    public Permissions Permission { get; } = permission;
}
