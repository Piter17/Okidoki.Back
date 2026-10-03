using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Services;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Okidoki.Api.Tests;

public class BaseIntegrationTest(ApiWebApplicationFactory factory,
    ITestOutputHelper output)
{
    protected ApiWebApplicationFactory Factory { get; } = factory;
    protected ITestOutputHelper Output { get; } = output;
    private HttpClient Client { get; } = factory.CreateClient();
    protected readonly IOnlineCache cache = factory.Services.GetRequiredService<IOnlineCache>();

    protected async Task ReinitCache()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var loader = scope.ServiceProvider.GetRequiredService<ICacheLoader>();
        await loader.InitAsync();
    }

    protected Task<UserClient> CreateUserInstance(AccessTokenResponse accessTokenResponse)
    {
        return UserClient.CreateAsync(Factory.CreateClient(), accessTokenResponse);
    }

    protected async Task<UserClient> RegisterAsync(string profileName)
    {
        var result = await Client.PostAsJsonAsync("/api/auth/registerAnonymus", new
        {
            ProfileName = profileName,
            GuildInvitationCode = (string?)null,
        });

        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var accessToken = await result.Content.ReadFromJsonAsync<AccessTokenResponse>();
        accessToken.Should().NotBeNull();

        return await CreateUserInstance(accessToken);
    }

    protected async Task<UserClient> LoginAsync(
        string email,
        string password)
    {
        using var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password,
        });

        var error = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Login failed with {response.StatusCode}: {error}");

        var accessToken = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        accessToken.Should().NotBeNull();

        return await CreateUserInstance(accessToken);
    }

    protected async Task DeleteAccountsAsync(params string[] profileNames)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Identity>>();
        var userIds = await db.Accounts
            .Where(x => profileNames.Contains(x.Nickname!))
            .Select(x => x.Id)
            .ToListAsync();

        var ownedGuilds = await db.Guilds
            .Where(x => userIds.Contains(x.OwnerId))
            .ToListAsync();
        db.Guilds.RemoveRange(ownedGuilds);
        await db.SaveChangesAsync();

        foreach (var userId in userIds)
        {
            var identity = await userManager.FindByIdAsync(userId.ToString());
            if (identity is not null)
            {
                var result = await userManager.DeleteAsync(identity);
                result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }
    }
}
