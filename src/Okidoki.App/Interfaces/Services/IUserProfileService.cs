using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Services;

public interface IUserProfileService
{
    Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken = default);
    Task<User?> UpdateProfileAsync(long userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<User?> UpdateAvatarAsync(long userId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);
}
