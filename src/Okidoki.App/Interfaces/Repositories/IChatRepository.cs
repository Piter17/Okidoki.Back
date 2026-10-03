using Okidoki.Domain.Entities;
using Okidoki.App.Contracts;

namespace Okidoki.App.Interfaces.Repositories;

public interface IChatRepository
{
    IQueryable<ChatMessage> MessagesForChannel(long guildChannelId);
    Task<long?> GetGuildIdForChannelAsync(long guildChannelId, CancellationToken cancellationToken = default);
    Task<ChatMessage?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        long guildChannelId,
        QueryDirection direction = QueryDirection.Prev,
        long? cursor = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task UpdateAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task DeleteAsync(ChatMessage message, CancellationToken cancellationToken = default);
}
