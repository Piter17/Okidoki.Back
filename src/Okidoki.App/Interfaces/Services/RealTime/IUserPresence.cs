namespace Okidoki.App.Interfaces.Services.RealTime;

public interface IUserPresence
{
    void Add(long userId, string connectionId);
    void Remove(long userId, string connectionId);
    IReadOnlySet<string> GetConnections(long userId);
    bool IsOnline(long userId);
}
