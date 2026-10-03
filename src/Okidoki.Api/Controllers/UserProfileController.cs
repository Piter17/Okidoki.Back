using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Mapping;
using Okidoki.Domain.Entities;

namespace Okidoki.Api.Controllers;

public class UserProfileController(IUserProfileService userProfileService, UserManager<Identity> userManager) : BaseController
{
    [HttpGet("GetUserProfile")]
    public async Task<Results<Ok<UserProfileDto>, NotFound>> GetByUserId(
        [FromQuery] long? userId = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await userProfileService.GetUserProfileAsync(userId ?? CurrentUserId!.Value, cancellationToken);
        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(UserProfileMapper.ToDto(entity));
    }


    [HttpPatch("profile")]
    public async Task<Results<Ok<UserProfileDto>, NotFound>> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var entity = await userProfileService.UpdateProfileAsync(CurrentUserId!.Value, request, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(UserProfileMapper.ToDto(entity));
    }

    [HttpPatch("SetAvatar")]
    public async Task<Results<Ok<UserProfileDto>, NotFound, BadRequest<string>>> UpdateAvatar(IFormFile avatar, CancellationToken cancellationToken)
    {
        if (avatar.Length == 0)
        {
            return TypedResults.BadRequest("A non-empty image file is required.");
        }

        await using var content = avatar.OpenReadStream();
        var entity = await userProfileService.UpdateAvatarAsync(CurrentUserId!.Value, content, avatar.FileName, avatar.ContentType, cancellationToken);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(UserProfileMapper.ToDto(entity));
    }
}
