using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Mapping;

public static class UserProfileMapper
{
    public static UserProfileDto ToDto(User entity, UserOnlineState? state = null)
    {
        return new UserProfileDto
        {
            Id = entity.Id,
            Nickname = entity.Nickname,
            State = state,
            ProfilePicture = entity.ProfilePicture,
            UserName = entity.Username,
        };
    }
}
