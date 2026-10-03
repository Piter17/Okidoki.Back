using System.Collections.Concurrent;
using Okidoki.App.Interfaces.Services.RealTime;

namespace Okidoki.Infrastructure.RealTime;

public class InMemoryUserPresence : IUserPresence
{
    readonly ConcurrentDictionary<long, HashSet<string>> _connections = new();

    public void Add(long userId, string connectionId)
    {
        _connections.AddOrUpdate(userId, [connectionId], (k, v) => {
            v.Add(connectionId);
            return v;
        } );
    }

    public void Remove(long userId, string connectionId)
    {
        _connections.AddOrUpdate(userId, [], (k, v) => {
            v.Remove(connectionId);
            return v;
        });
    }

    public IReadOnlySet<string> GetConnections(long userId)
    {
        return _connections.TryGetValue(userId, out var connections) ? connections : [];
    }

    public bool IsOnline(long userId)
    {
        return GetConnections(userId).Any();
    }
}
