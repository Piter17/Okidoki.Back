using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using IdGen;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Okidoki.App.Contracts;using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Services;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Exceptions;

namespace Okidoki.Api.Controllers;

[ApiController]
[Route(AuthController.ControllerRoute)]
public sealed class AuthController(
        IIdGenerator<long> _idGenerator,
        UserManager<Identity> _userManager,
        IUserStore<Identity> _userStore,
        SignInManager<Identity> _signInManager,
        IEmailSender<Identity> _emailSender,
        IUserService _userService,
        ICacheLoader _cacheLoader,
        LinkGenerator _linkGenerator
    ) : ControllerBase
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    public const string ControllerRoute = "api/auth";
    private const string _confirmEmailRoute = $"{ControllerRoute}/confirmEmail";
    private const string _confirmEmailEndpointName = $"MapIdentityApi-{_confirmEmailRoute}";

    private IUserEmailStore<Identity> _emailStore => (IUserEmailStore<Identity>)_userStore;

    // other methods are mapped in .MapIdentityApi<TUser>()
    [AllowAnonymous]
    [HttpPost("registerUser")]
    public async Task<Results<Created<RegisterResponse>, ValidationProblem>>
        RegisterUser(RegistrationRequest request)
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("(MapIdentityApi) requires a user store with email support.");
        }

        var email = request.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            return CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email)));
        }

        var userId = _idGenerator.CreateId();
        var userName = await _userService.GetFreeUserNameAsync(request.ProfileName);

        var user = new Identity
        {
            Id = userId,
            User = new User()
            {
                Id = userId,
                Username = userName,
                Nickname = request.ProfileName,
            }
        };
        await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
        try
        {
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await SendConfirmationEmailAsync(user, HttpContext, email);

            await _cacheLoader.InitUserAsync(userId);
            return TypedResults.Created($"api/auth/users/{userId}", new RegisterResponse
            {
                UserId = userId,
                UserName = userName,
            });
        }
        catch (DbUpdateException exception)
            when (exception.IsUniqueConstraintViolation("FK_UserProfiles_Accounts_UserId"))
        {
            return CreateValidationProblem(IdentityResult.Failed(
            new IdentityError
            {
                Code = "DuplicateUserName",
                Description = $"Username `{userName}` is already taken.",
            }
            ));
        }
    }

    [AllowAnonymous]
    [HttpPost("registerAnonymus")]
    [ProducesResponseType(typeof(AccessTokenResponse), 200)]
    public async Task<Results<EmptyHttpResult, ValidationProblem>> Register(AnonymusRegistrationRequest request)
    {
        var profileName = request.ProfileName.Trim();
        var userId = _idGenerator.CreateId();
        var username = await _userService.GetFreeUserNameAsync(profileName);
        var user = new Identity
        {
            Id = userId,
            User = new User
            {
                Id = userId,
                Username = username,
                Nickname = profileName,
            }
        };
        await _userStore.SetUserNameAsync(user, username, CancellationToken.None);

        try
        {
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            await _signInManager.SignInAsync(user, isPersistent: false);

            await _cacheLoader.InitUserAsync(userId);

            return TypedResults.Empty;
        }
        catch (DbUpdateException exception)
            when (exception.IsUniqueConstraintViolation("FK_UserProfiles_Accounts_UserId"))
        {
            return CreateValidationProblem(IdentityResult.Failed(
                new IdentityError
                {
                    Code = "DuplicateUserName",
                    Description = $"Username `{request.ProfileName}` is already taken.",
                }
            ));
        }
    }

    [Authorize]
    [HttpPost("credentials")]
    public async Task<Results<NoContent, ValidationProblem>> AddCredentials(AddCredentialsRequest request)
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("This user store does not support email addresses.");
        }

        if (string.IsNullOrEmpty(request.Email) || !_emailAddressAttribute.IsValid(request.Email))
        {
            return CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(request.Email)));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["User"] = ["The authenticated user could not be found."],
            });
        }

        var usernameResult = await _userManager.SetUserNameAsync(user, request.Email);
        if (!usernameResult.Succeeded)
        {
            return CreateValidationProblem(usernameResult);
        }

        var passwordResult = await _userManager.AddPasswordAsync(user, request.Password);
        if (!passwordResult.Succeeded)
        {
            return CreateValidationProblem(passwordResult);
        }

        var emailResult = await _userManager.SetEmailAsync(user, request.Email);
        if (!emailResult.Succeeded)
        {
            return CreateValidationProblem(emailResult);
        }

        await SendConfirmationEmailAsync(user, HttpContext, request.Email);

        return TypedResults.NoContent();
    }

    private async Task SendConfirmationEmailAsync(Identity user, HttpContext context, string email, bool isChange = false)
    {
        var code = isChange
            ? await _userManager.GenerateChangeEmailTokenAsync(user, email)
            : await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var userId = await _userManager.GetUserIdAsync(user);
        var routeValues = new RouteValueDictionary()
        {
            ["userId"] = userId,
            ["code"] = code,
        };

        if (isChange)
        {
            // This is validated by the /confirmEmail endpoint on change.
            routeValues.Add("changedEmail", email);
        }

        var confirmEmailUrl = _linkGenerator.GetUriByName(context, _confirmEmailEndpointName, routeValues)
            ?? throw new NotSupportedException($"Could not find endpoint named '{_confirmEmailEndpointName}'.");

        await _emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}
