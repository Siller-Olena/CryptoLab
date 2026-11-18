using System.Collections.Concurrent;
using CryptoLab.Api.Options;
using CryptoLab.Common;
using Microsoft.Extensions.Options;

namespace CryptoLab.Api;

public class Session
{
    public string Id { get; set; } = "";
    public AesKey Key { get; set; } = default!;
    public DateTime ExpiredAt { get; set; }
    public bool IsValid { get; set; } = true;
}

public class SessionStorage
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private readonly SessionSettings _options;

    public SessionStorage(IOptions<SessionSettings> options)
    {
        _options = options.Value;
    }

    public void Set(string sessionId, AesKey key)
    {
        _sessions[sessionId] = new Session
        {
            Id = sessionId,
            Key = key,
            ExpiredAt = DateTime.UtcNow.AddSeconds(_options.ExpiresInSeconds),
            IsValid = true
        };
    }

    public AesKey? Get(string sessionId)
        => _sessions.TryGetValue(sessionId, out var s) ? s.Key : null;

    public Session? GetSession(string sessionId)
        => _sessions.TryGetValue(sessionId, out var s) ? s : null;

    public IEnumerable<Session> GetAll() => _sessions.Values;
}