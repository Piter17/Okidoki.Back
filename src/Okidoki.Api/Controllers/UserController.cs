using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.Domain.Entities;

namespace Okidoki.Api.Controllers;

public class UserController(UserManager<Identity> _userManager, IUserService userService)
    : BaseController
{
        [HttpGet()]
    public async Task<Results<Ok<IdentityDto>, NotFound>> GetMe()
    {
        if (await _userManager.GetUserAsync(User) is not { } user)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(new IdentityDto()
        {
            Email = await _userManager.GetEmailAsync(user) ?? null,
            UserId = long.Parse(await _userManager.GetUserIdAsync(user) ?? throw new NotSupportedException("Users must have an id.")),
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
        });
    }

    [HttpPatch("SetUserName")]
    public async Task<Results<NoContent, NotFound>> UpdateUsername([FromQuery] string username, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateUsernameAsync(CurrentUserId!.Value, username, cancellationToken);
        return user is null ? TypedResults.NotFound() : TypedResults.NoContent();
    }
}