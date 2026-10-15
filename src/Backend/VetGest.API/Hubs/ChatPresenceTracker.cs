using System.Collections.Concurrent;

namespace VetGest.API.Hubs;

public sealed class ChatPresenceTracker : IChatPresenceTracker
{
    private readonly ConcurrentDictionary<string, int> _connections = new();

    public void Connected(string userId) => _connections.AddOrUpdate(userId, 1, (_, count) => count + 1);

    public void Disconnected(string userId)
    {
        while (_connections.TryGetValue(userId, out var count))
        {
            if (count <= 1 ? _connections.TryRemove(new KeyValuePair<string, int>(userId, count)) : _connections.TryUpdate(userId, count - 1, count))
                return;
        }
    }

    public bool IsOnline(string userId) => _connections.ContainsKey(userId);
}
