using AwesomeAssertions;

namespace Okidoki.Api.Tests;

public sealed class AuthControllerTests(ApiWebApplicationFactory factory,
    ITestOutputHelper output)
    : BaseIntegrationTest(factory, output),
        IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task OnlineCacheTest()
    {
        var dump = cache.Dump();
        dump.Guilds.Should().BeEmpty();
        dump.Users.Should().BeEmpty();

        var tUsers = await Task.WhenAll(
            Enumerable.Range(1, 10)
                .Select((x) => RegisterAsync($"usr_{x}")));

        var userIds = tUsers.Select(x => x.UserId);
        Output.WriteLine($"userIds: {string.Join(", ", userIds)}");

        dump = cache.Dump();
        userIds.Should().HaveCount(10).And.BeEquivalentTo(dump.Users.Keys);

        var g1 = await tUsers[0].CreateGuild("g_1");
        g1.Name.Should().Be("g_1");

        dump = cache.Dump();
        dump.Guilds.Should().ContainKey(g1.Id);
        dump.GetGuildUsers(g1.Id).Should().Contain(tUsers[0].UserId);

        var g1i = await tUsers[0].CreateGuildInvitation(g1.Id, 3);
        g1i.Code.Should().HaveLength(10);

        await tUsers[1].JoinGuild(g1i.Code);

        dump = cache.Dump();
        dump.GetGuildUsers(g1.Id).Should().Contain([tUsers[0].UserId, tUsers[1].UserId]);

        await tUsers[2].JoinGuild(g1i.Code);
        await tUsers[3].JoinGuild(g1i.Code);

        dump = cache.Dump();
        dump.GetGuildUsers(g1.Id).Should().Contain([tUsers[0].UserId, tUsers[1].UserId, tUsers[2].UserId, tUsers[3].UserId]);

        Output.WriteLine($"g1 roles: {string.Join(", ", dump.Guilds[g1.Id].Roles)}");

        var g1_1 = await tUsers[0].GetGuild(g1.Id);

        Output.WriteLine($"g1 roles: {string.Join(", ", g1_1.Roles.Select(x => $"{x.Name} - {x.Id}"))}");
    }
}
