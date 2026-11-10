using System.Collections.Concurrent;

namespace Api.Hubs;

public sealed record SalesRequestConnectionEntry(int UserId, IReadOnlySet<string> Groups);

public class SalesRequestConnectionRegistry
{
    private readonly ConcurrentDictionary<string, SalesRequestConnectionEntry> _connections = new(StringComparer.Ordinal);

    public int Count => _connections.Count;

    public void Set(string connectionId, int userId, IReadOnlySet<string> groups)
        => _connections[connectionId] = new SalesRequestConnectionEntry(userId, groups);

    public bool TryGet(string connectionId, out SalesRequestConnectionEntry? entry)
        => _connections.TryGetValue(connectionId, out entry);

    public void Remove(string connectionId)
        => _connections.TryRemove(connectionId, out _);

    public IReadOnlyList<string> GetConnectionIds(int userId)
        => _connections.Where(x => x.Value.UserId == userId).Select(x => x.Key).ToList();
}
