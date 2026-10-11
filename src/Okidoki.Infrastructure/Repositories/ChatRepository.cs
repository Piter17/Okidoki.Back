using Microsoft.EntityFrameworkCore;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;

namespace Okidoki.Infrastructure.Repositories;

public class ChatRepository(AppDbContext context) : IChatRepository
{
    public IQueryable<ChatMessage> MessagesForChannel(long guildChannelId)
        => context.ChatMessages
            .Where(x => x.GuildChannelId == guildChannelId)
            .AsNoTracking();

    public Task<long?> GetGuildIdForChannelAsync(long guildChannelId, CancellationToken cancellationToken = default)
        => context.GuildChannels
            .Where(channel => channel.Id == guildChannelId)
            .Select(channel => (long?)channel.GuildId)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<ChatMessage?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default)
        => context.ChatMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        long guildChannelId,
        QueryDirection direction = QueryDirection.Prev,
        long? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ChatMessages
            .Where(x => x.GuildChannelId == guildChannelId);

        if (cursor.HasValue)
        {
            query = direction == QueryDirection.Next
                ? query.Where(x => x.Id > cursor.Value)
                : query.Where(x => x.Id < cursor.Value);
        }

        query = direction == QueryDirection.Next
            ? query.OrderBy(x => x.Id)
            : query.OrderByDescending(x => x.Id);

        return await query
            .Take(30)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await context.ChatMessages.AddAsync(message, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        context.ChatMessages.Update(message);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        context.ChatMessages.Remove(message);
        await context.SaveChangesAsync(cancellationToken);
    }
}
