namespace Okidoki.App.Interfaces.Services.RealTime;

public interface IRealTimeGroupManager
{
    Task AddUserToGuildGroupAsync(
        long userId,
        long guildId,
        CancellationToken cancellationToken = default);
}