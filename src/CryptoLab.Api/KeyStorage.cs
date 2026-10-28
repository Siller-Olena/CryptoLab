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

    public (int Id, RsaKeys Keys) GetRandom()
    {
        var ids = _keys.Keys.ToArray();
        var id = ids[Random.Shared.Next(ids.Length)];
        return (id, _keys[id]);
    }
}