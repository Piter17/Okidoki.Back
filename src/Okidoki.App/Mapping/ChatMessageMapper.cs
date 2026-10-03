using Okidoki.App.Contracts;
using Okidoki.Domain.Entities;

namespace Okidoki.App.Mapping;

public static class ChatMessageMapper
{
    public static ChatMessageDto ToDto(ChatMessage entity)
    {
        return new ChatMessageDto
        {
            Id = entity.Id,
            GuildChannelId = entity.GuildChannelId,
            SenderId = entity.SenderId,
            Content = entity.Content,
            SendTime = entity.SendTime,
            ModifiedTime = entity.ModifiedTime,
        };
    }
}
