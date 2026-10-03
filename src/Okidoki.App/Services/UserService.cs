using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Services;

public class UserService(IUserRepository userRepository,
    IRealTimeNotifier realTime) : IUserService
{
    public Task<string> GetFreeUserNameAsync(string profileName) => userRepository.GetFreeUserNameAsync(profileName);

    public async Task<User?> UpdateUsernameAsync(long userId, string username, CancellationToken cancellationToken = default)
    {
        if (await userRepository.IsUserNameUsedAsync(username))
            return null;

        var user = await userRepository.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Username = username.Trim();
        await userRepository.UpdateUserAsync(user, cancellationToken);
        _ = realTime.UserProfileChangedAsync(user, cancellationToken);

        return user;
    }
}