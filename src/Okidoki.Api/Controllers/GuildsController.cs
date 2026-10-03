using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.App.Mapping;

namespace Okidoki.Api.Controllers;

public class GuildsController(IGuildService service, IUserPresence userPresence) : BaseController
{
    [HttpGet("{guildId:long}")]
    public async Task<Results<Ok<GuildDto>, NotFound>> GetGuildById(long guildId, CancellationToken cancellationToken)
    {
        var entity = await service.GetGuildAsync(guildId, CurrentUserId!.Value, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpGet("user")]
    public async Task<Results<Ok<List<GuildProfileDto>>, BadRequest>> GetGuildsForUser(CancellationToken cancellationToken)
    {
        var entities = await service.GetGuildsAsync(CurrentUserId!.Value, cancellationToken);
        return TypedResults.Ok(entities.ConvertAll(GuildMapper.ToDto));
    }

    [HttpGet("{guildId:long}/userList")]
    public async Task<Results<Ok<List<UserProfileDto>>, BadRequest>> GetUserList(long guildId, CancellationToken cancellationToken)
    {
        var entities = await service.GetUsersAsync(CurrentUserId!.Value, guildId, cancellationToken);
        return TypedResults.Ok(entities.Select(x => UserProfileMapper.ToDto(x, userPresence.IsOnline(x.Id) ? UserOnlineState.Online : UserOnlineState.Offline)).ToList());
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<Results<Ok<GuildDto>, BadRequest<string>>> CreateGuild(
        [FromForm] CreateGuildRequest request,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Guild name is required.");
        }

        if ((image?.Length == 0))
        {
            return TypedResults.BadRequest("A non-empty image file is required.");
        }

        await using var content = image?.OpenReadStream();
        var entity = await service.CreateGuildAsync(
            CurrentUserId!.Value,
            request.Name,
            content,
            image?.FileName,
            image?.ContentType,
            cancellationToken);
        return TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpPost("{guildId:long}/invites")]
    public async Task<Results<Ok<GuildInvitationDto>, NotFound>> CreateInvitation(
        long guildId,
        [FromBody] CreateGuildInvitationRequest? request,
        CancellationToken cancellationToken)
    {
        var entity = await service.CreateInvitationAsync(
            guildId,
            CurrentUserId!.Value,
            request?.ValidUntil,
            request?.MaxUses,
            cancellationToken);
        return TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpGet("invites/{invitationCode}")]
    public async Task<Results<Ok<GuildInvitationDto>, NotFound>> GetInvitation(string invitationCode, CancellationToken cancellationToken)
    {
        var entity = await service.GetInvitationAsync(invitationCode, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpPost("invites/{invitationCode}/accept")]
    public async Task<Results<Ok<GuildDto>, NotFound>> AcceptInvitation(string invitationCode, CancellationToken cancellationToken)
    {
        var entity = await service.AcceptInvitationAsync(CurrentUserId!.Value, invitationCode, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpDelete("{guildId:long}/membership")]
    public async Task<Results<NoContent, NotFound>> LeaveGuild(long guildId, CancellationToken cancellationToken)
    {
        return await service.LeaveGuildAsync(CurrentUserId!.Value, guildId, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    [HttpDelete("{guildId:long}/users/{userId:long}")]
    public async Task<Results<NoContent, NotFound>> KickUser(long guildId, long userId, CancellationToken cancellationToken)
    {
        return await service.KickUserAsync(CurrentUserId!.Value, guildId, userId, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    [HttpPost("{guildId:long}/channels")]
    public async Task<Results<Ok<GuildChannelDto>, NotFound>> CreateChannel(
        long guildId,
        [FromBody] CreateGuildChannelRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await service.CreateChannelAsync(
            CurrentUserId!.Value,
            guildId,
            request.GroupId,
            request.Name,
            request.Type,
            cancellationToken);
        return TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpDelete("{guildId:long}/channels/{channelId:long}")]
    public async Task<Results<NoContent, NotFound>> RemoveChannel(long guildId, long channelId, CancellationToken cancellationToken)
    {
        return await service.RemoveChannelAsync(CurrentUserId!.Value, guildId, channelId, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    [HttpPost("{guildId:long}/roles")]
    public async Task<Results<Ok<GuildRoleDto>, NotFound>> CreateRole(
        long guildId,
        [FromBody] CreateGuildRoleRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await service.CreateRoleAsync(guildId, request.Name, cancellationToken);
        return TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpDelete("roles/{roleId:long}")]
    public async Task<Results<NoContent, NotFound>> RemoveRole(long roleId, CancellationToken cancellationToken)
    {
        return await service.RemoveRoleAsync(roleId, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    [HttpPut("roles/{roleId:long}/users/{userId:long}")]
    public async Task<Results<NoContent, NotFound>> AssignRole(long roleId, long userId, CancellationToken cancellationToken)
    {
        return await service.AssignRoleAsync(userId, roleId, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    [HttpDelete("roles/{roleId:long}/users/{userId:long}")]
    public async Task<Results<NoContent, NotFound>> UnassignRole(long roleId, long userId, CancellationToken cancellationToken)
    {
        return await service.UnassignRoleAsync(userId, roleId, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    [HttpPatch("{guildId:long}/name")]
    public async Task<Results<Ok<GuildProfileDto>, NotFound>> UpdateName(
        long guildId,
        [FromBody] UpdateGuildNameRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await service.UpdateNameAsync(guildId, request.Name, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(GuildMapper.ToDto(entity));
    }

    [HttpPatch("{guildId:long}/image")]
    [Consumes("multipart/form-data")]
    public async Task<Results<Ok<GuildProfileDto>, NotFound, BadRequest<string>>> UpdateImage(long guildId, IFormFile image, CancellationToken cancellationToken)
    {
        if (image?.Length == 0)
        {
            return TypedResults.BadRequest("A non-empty image file is required.");
        }

        await using var content = image!.OpenReadStream();
        var entity = await service.UpdateImageAsync(guildId, content, image.FileName, image.ContentType, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(GuildMapper.ToDto(entity));
    }
}
