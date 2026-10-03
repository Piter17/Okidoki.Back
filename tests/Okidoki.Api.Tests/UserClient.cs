using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Okidoki.App.Contracts;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Okidoki.Api.Tests;

public sealed class UserClient
{
    public long UserId {get; private set;}
    public string? Email {get; private set;}
    private String RefreshToken { get; }
    private HttpClient Client {get; init;}
    private UserClient(HttpClient client, AccessTokenResponse tokenResponse)
    {
        Client = client;
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        RefreshToken = tokenResponse.RefreshToken;
    }

    public static async Task<UserClient> CreateAsync(HttpClient client, AccessTokenResponse tokenResponse)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var user = new UserClient(client, tokenResponse);
        await user.GetUserInfo();

        return user;
    }

    public async Task AddCredentials(string email, string password)
    {
        var result = await Client.PostAsJsonAsync(
            "/api/auth/credentials",
            new
            {
                Email = email,
                Password = password,
            });

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public async Task<IdentityDto> GetUserInfo()
    {
        var result = await Client.GetAsync("/api/user");
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var userInfo = await result.Content.ReadFromJsonAsync<IdentityDto>();
        userInfo.Should().NotBeNull();

        UserId = userInfo.UserId;
        Email = userInfo.Email;

        return userInfo;
    }

    public async Task<GuildDto> CreateGuild(string name)
    {
        using var request = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
        };
        using HttpResponseMessage result = await Client.PostAsync("/api/guilds", request);
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var guild = await result.Content.ReadFromJsonAsync<GuildDto>();

        guild.Should().NotBeNull();
        guild.Name.Should().Be(name);
        guild.OwnerId.Should().Be(UserId);

        return guild;
    }

    public async Task<GuildInvitationDto> CreateGuildInvitation(long guildId, int maxUses)
    {
        using var result = await Client.PostAsJsonAsync(
            $"/api/guilds/{guildId}/invites",
            new { MaxUses = maxUses });

        var invitation = await result.Content.ReadFromJsonAsync<GuildInvitationDto>();

        invitation.Should().NotBeNull();
        invitation.Guild.Id.Should().Be(guildId);
        invitation.Code.Should().NotBeNullOrEmpty();

        return invitation;
    }

    public async Task<GuildDto> JoinGuild(string code)
    {
        using var result = await Client.PostAsync(
            $"/api/Guilds/invites/{code}/accept", null);

        var guild = await result.Content.ReadFromJsonAsync<GuildDto>();
        guild.Should().NotBeNull();

        return guild;
    }

    public async Task<GuildDto> GetGuild(long id)
    {
        using var result = await Client.GetAsync(
            $"/api/Guilds/{id}");

        var guild = await result.Content.ReadFromJsonAsync<GuildDto>();
        guild.Should().NotBeNull();
        guild.Id.Should().Be(id);

        return guild;
    }
}