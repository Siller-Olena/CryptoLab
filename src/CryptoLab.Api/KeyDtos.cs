using CryptoLab.Common;

namespace CryptoLab.Api;

public record KeyNameRequest(string? Name);

public record KeyDto(int Id, string Name, string PublicKey)
{
    public static KeyDto From(int id, RsaKeys keys) =>
        new(id, string.IsNullOrWhiteSpace(keys.Name) ? $"Key {id}" : keys.Name, keys.PublicKey);
}