using System.Security.Cryptography;
using System.Text;

namespace CryptoLab.Common;

public interface IHashService
{
    string ComputeHash(string data);
    bool Verify(string data, string hash);
}

public class HashService : IHashService
{
    public string ComputeHash(string data)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data)));

    public bool Verify(string data, string hash)
    {
        byte[] received;
        try { received = Convert.FromBase64String(hash); }
        catch (FormatException) { return false; }

        var computed = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return CryptographicOperations.FixedTimeEquals(computed, received);
    }
}