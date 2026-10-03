using Microsoft.AspNetCore.Mvc;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Mapping;

namespace Okidoki.Api.Controllers;

public class ChatController(IChatService service) : BaseController
{
    [HttpGet("Messages/{guildChannelId:long}")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(
        long guildChannelId,
        [FromQuery] long? cursor,
        [FromQuery] QueryDirection? direction,
        CancellationToken cancellationToken = default)
    {
        var dir = direction ?? QueryDirection.Prev;
        var entities = await service.GetMessagesAsync(guildChannelId, dir, cursor, cancellationToken);
        return Ok(entities.Select(entity => ChatMessageMapper.ToDto(entity)).ToList());
    }

    [HttpPost("{guildChannelId:long}")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(long guildChannelId, [FromBody] CreateChatMessageRequest request, CancellationToken cancellationToken)
    {
        var entity = await service.AddMessageAsync(CurrentUserId!.Value, guildChannelId, request.Content, cancellationToken);
        return Ok(ChatMessageMapper.ToDto(entity));
    }

    [HttpPatch("{messageId:long}")]
    public async Task<ActionResult<ChatMessageDto>> EditMessage(long messageId, [FromBody] UpdateChatMessageRequest request, CancellationToken cancellationToken)
    {
        var entity = await service.EditMessageAsync(CurrentUserId!.Value, messageId, request.Content, cancellationToken);
        return Ok(ChatMessageMapper.ToDto(entity));
    }

    [HttpDelete("{messageId:long}")]
    public async Task<IActionResult> DeleteMessage(long messageId, CancellationToken cancellationToken)
    {
        await service.DeleteMessageAsync(CurrentUserId!.Value, messageId, cancellationToken);
        return NoContent();
    }
}
