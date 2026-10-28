using System.Collections.Concurrent;
using CryptoLab.Common;

namespace CryptoLab.Api;

public class SessionStorage
{
    private readonly ConcurrentDictionary<string, AesKey> _sessions = new();

    public void Set(string sessionId, AesKey key) => _sessions[sessionId] = key;
    public AesKey? Get(string sessionId) => _sessions.TryGetValue(sessionId, out var k) ? k : null;
}