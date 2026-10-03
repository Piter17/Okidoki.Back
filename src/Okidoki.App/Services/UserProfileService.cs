using IdGen;
using Microsoft.EntityFrameworkCore;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Services;

public class UserProfileService(IUserRepository userRepository, IImageStorageService imageStorage, IIdGenerator<long> idGenerator, IRealTimeNotifier realTime) : IUserProfileService
{
    public async Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserAsync(userId, cancellationToken);
        return user;
    }

    public async Task<User?> UpdateProfileAsync(long userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await userRepository.GetUserAsync(userId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        profile.Nickname = request.ProfileName;

        await userRepository.UpdateUserAsync(profile, cancellationToken);
        return profile;
    }

    
    public async Task<User?> UpdateAvatarAsync(long userId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var profile = await userRepository.GetUserAsync(userId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var result = await imageStorage.UploadUserAvatarAsync(userId, content, fileName, contentType, cancellationToken);
        if (!result.Success && result.ResourceUri is null)
        {
            throw new InvalidOperationException("Avatar upload failed.", result.Error);
        }

        profile.ProfilePicture = result.ResourceUri!.ToString();
        await userRepository.UpdateUserAsync(profile, cancellationToken);
        _ = realTime.UserProfileChangedAsync(profile, cancellationToken);

        return profile;
    }
}
