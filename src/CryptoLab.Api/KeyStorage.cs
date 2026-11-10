using System.Collections.Concurrent;
using CryptoLab.Common;

namespace CryptoLab.Api;

public class KeyStorage
{
    private readonly ConcurrentDictionary<int, RsaKeys> _keys = new();
    private int _nextId = 0;

    public int Add(RsaKeys keys)
    {
        var id = Interlocked.Increment(ref _nextId);
        _keys[id] = keys;
        return id;
    }

    public RsaKeys? Get(int id) => _keys.TryGetValue(id, out var k) ? k : null;

    public IReadOnlyList<(int Id, RsaKeys Keys)> List() =>
        _keys.OrderBy(k => k.Key).Select(k => (k.Key, k.Value)).ToList();

    public bool Rename(int id, string name)
    {
        if (!_keys.TryGetValue(id, out var keys)) return false;
        keys.Name = name;
        return true;
    }

    public bool Remove(int id) => _keys.TryRemove(id, out _);

    public (int Id, RsaKeys Keys)? GetRandom()
    {
        var ids = _keys.Keys.ToArray();
        if (ids.Length == 0) return null;
        var id = ids[Random.Shared.Next(ids.Length)];
        return (id, _keys[id]);
    }
}