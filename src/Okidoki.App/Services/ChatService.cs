using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.App.Contracts;
using Okidoki.App.Mapping;
using Okidoki.Domain.Entities;
using IdGen;

namespace Okidoki.App.Services;

public class ChatService(
    IChatRepository chatRepository,
    IRealTimeNotifier realTimeNotifier,
    IIdGenerator<long> idGenerator) : IChatService
{
    public async Task<ChatMessage> AddMessageAsync(long userId, long guildChannelId, string content, CancellationToken cancellationToken = default)
    {
        var guildId = await chatRepository.GetGuildIdForChannelAsync(guildChannelId, cancellationToken)
            ?? throw new InvalidOperationException($"Channel {guildChannelId} not found.");

        var message = new ChatMessage
        {
            Id = idGenerator.CreateId(),
            GuildChannelId = guildChannelId,
            SenderId = userId,
            Content = content,
            SendTime = DateTime.UtcNow,
        };

        await chatRepository.AddAsync(message, cancellationToken);

        await realTimeNotifier.AddMessageAsync(
            guildId, message, cancellationToken);

        return message;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        long guildChannelId,
        QueryDirection direction = QueryDirection.Prev,
        long? cursor = null,
        CancellationToken cancellationToken = default)
    {
        return await chatRepository.GetMessagesAsync(guildChannelId, direction, cursor, cancellationToken);
    }

    public async Task DeleteMessageAsync(long userId, long messageId, CancellationToken cancellationToken = default)
    {
        var message = await chatRepository.GetMessageAsync(messageId, cancellationToken);

        if (message is null || message.SenderId != userId)
        {
            return;
        }

        var guildId = await chatRepository.GetGuildIdForChannelAsync(message.GuildChannelId, cancellationToken);
        await chatRepository.DeleteAsync(message, cancellationToken);

        if (guildId.HasValue)
        {
            await realTimeNotifier.DeleteMessageAsync(guildId.Value, messageId, cancellationToken);
        }
    }

    public async Task<ChatMessage> EditMessageAsync(long userId, long messageId, string content, CancellationToken cancellationToken = default)
    {
        var message = await chatRepository.GetMessageAsync(messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {messageId} not found.");

        if (message.SenderId != userId)
        {
            throw new UnauthorizedAccessException("Only the message author can edit it.");
        }

        message.Content = content;
        message.ModifiedTime = DateTime.UtcNow;
        await chatRepository.UpdateAsync(message, cancellationToken);
        var guildId = await chatRepository.GetGuildIdForChannelAsync(message.GuildChannelId, cancellationToken);
        if (guildId.HasValue)
        {
            await realTimeNotifier.EditMessageAsync(guildId.Value, message, cancellationToken);
        }

        return message;
    }
}
