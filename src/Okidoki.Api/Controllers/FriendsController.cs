using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Mapping;

namespace Okidoki.Api.Controllers;

public class FriendsController(IFriendService service) : BaseController
{
    [HttpGet]
    public async Task<Results<Ok<List<FriendDto>>, BadRequest>> GetFriends(CancellationToken cancellationToken)
    {
        var entities = await service.GetFriendsAsync(CurrentUserId!.Value, cancellationToken);
        return TypedResults.Ok(entities.ConvertAll(entity => FriendMapper.ToDto(entity, DateTime.UtcNow)));
    }

    [HttpGet("receivedRequests")]
    public async Task<Results<Ok<List<ReceivedRequestDto>>, BadRequest>> GetReceivedRequestsAsync(CancellationToken cancellationToken)
    {
        var entities = await service.GetReceivedRequestsAsync(CurrentUserId!.Value, cancellationToken);
        return TypedResults.Ok(entities.ConvertAll(FriendMapper.ToReceivedDto));
    }

    [HttpGet("sentRequests")]
    public async Task<Results<Ok<List<SentRequestDto>>, BadRequest>> GetSentRequestsAsync(CancellationToken cancellationToken)
    {
        var entities = await service.GetSentRequestsAsync(CurrentUserId!.Value, cancellationToken);
        return TypedResults.Ok(entities.ConvertAll(FriendMapper.ToSentDto));
    }

    [HttpPost("sendInviteById")]
    public async Task<Results<Ok<SentRequestDto>, BadRequest>> SendRequestById([FromQuery] long userId, CancellationToken cancellationToken)
    {
        var entity = await service.SendRequestAsync(CurrentUserId!.Value, userId, cancellationToken);
        return TypedResults.Ok(FriendMapper.ToSentDto(entity));
    }

    [HttpPost("sendInvite")]
    public async Task<Results<Ok<SentRequestDto>, BadRequest>> SendRequestByUsername([FromQuery] string username, CancellationToken cancellationToken)
    {
        var entity = await service.SendRequestAsync(CurrentUserId!.Value, username, cancellationToken);
        return TypedResults.Ok(FriendMapper.ToSentDto(entity));
    }

    [HttpPut("revokeRequest")]
    public async Task<Results<NoContent, BadRequest>> RevokeRequest([FromQuery] long targetId, CancellationToken cancellationToken)
    {
        await service.RevokeRequestAsync(CurrentUserId!.Value, targetId, cancellationToken);
        return TypedResults.NoContent();
    }

    [HttpPut("acceptRequest")]
    public async Task<Results<NoContent, BadRequest>> AcceptRequest([FromQuery] long senderId, CancellationToken cancellationToken)
    {
        await service.AcceptRequestAsync(CurrentUserId!.Value, senderId, cancellationToken);
        return TypedResults.NoContent();
    }

    [HttpPut("declineRequest")]
    public async Task<Results<NoContent, BadRequest>> DeclineRequest([FromQuery] long senderId, CancellationToken cancellationToken)
    {
        await service.DeclineRequestAsync(CurrentUserId!.Value, senderId, cancellationToken);
        return TypedResults.NoContent();
    }
}
