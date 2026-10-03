using Okidoki.Domain.Authorization;

namespace Okidoki.App.Interfaces.Services;

public interface IGuildAuthorizationService
{
    Task EnsurePermissionAsync(
        long userId,
        long guildId,
        Permissions permission,
        CancellationToken cancellationToken = default);
}
