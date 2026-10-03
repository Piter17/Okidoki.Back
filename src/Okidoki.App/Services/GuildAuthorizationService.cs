using Okidoki.App.Exceptions;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.Domain.Authorization;

namespace Okidoki.App.Services;

public sealed class GuildAuthorizationService(IGuildRepository guildRepository) : IGuildAuthorizationService
{
    public async Task EnsurePermissionAsync(
        long userId,
        long guildId,
        Permissions permission,
        CancellationToken cancellationToken = default)
    {
        var membership = await guildRepository.GetMembershipAsync(userId, guildId, cancellationToken);
        if (membership is null)
        {
            throw new UnauthorizedAccessException("The user is not a member of this guild.");
        }

        if (membership.Guild.OwnerId == userId)
        {
            return;
        }

        var permissions = membership.Roles.Aggregate(
            Permissions.None,
            (current, role) => current | (Permissions)role.Permissions);

        if ((permissions & permission) != permission)
        {
            throw new GuildPermissionDeniedException(permission);
        }
    }
}
