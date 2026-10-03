using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Mapping;

public static class FriendMapper
{
    public static FriendDto ToDto(User entity, DateTime createdAt)
    {
        return new FriendDto
        {
            UserId = entity.Id,
            UserName = entity.Username,
            Nickname = entity.Nickname,
            CreatedAt = createdAt,
        };
    }

    public static ReceivedRequestDto ToReceivedDto(FriendRequest entity)
    {
        return new ReceivedRequestDto
        {
            SenderId = entity.SenderId,
            Sender = UserProfileMapper.ToDto(entity.Sender),
            CreatedAt = entity.CreatedAt,
        };
    }

    public static SentRequestDto ToSentDto(FriendRequest entity)
    {
        return new SentRequestDto
        {
            TargetId = entity.TargetId,
            Target = UserProfileMapper.ToDto(entity.Target),
            CreatedAt = entity.CreatedAt,
        };
    }
}
