using Okidoki.Domain.Entities;
using Okidoki.App.Contracts;

namespace Okidoki.App.Interfaces.Services;

public interface IChatService
{
    Task<ChatMessage> AddMessageAsync(long userId, long guildChannelId, string content, CancellationToken cancellationToken = default);
    Task<ChatMessage> EditMessageAsync(long userId, long messageId, string content, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        long guildChannelId,
        QueryDirection direction = QueryDirection.Prev,
        long? cursor = null,
        CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(long userId, long messageId, CancellationToken cancellationToken = default);
}
