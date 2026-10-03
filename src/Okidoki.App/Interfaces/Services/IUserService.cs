using Okidoki.Domain.Entities;

namespace Okidoki.App.Interfaces.Services;

public interface IUserService
{
    Task<string> GetFreeUserNameAsync(string profileName);
    Task<User?> UpdateUsernameAsync(long userId, string username, CancellationToken cancellationToken = default);
}