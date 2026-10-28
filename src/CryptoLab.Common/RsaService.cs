using System.Security.Cryptography;
using System.Text;

namespace CryptoLab.Common;

public class RsaService : IRsaService
{
    public RsaKeys GenerateCryptoKeys()
    {
        using var rsa = RSA.Create(2048);
        return new RsaKeys
        {
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
        };
    }

    public string Encrypt(string publicKey, string plainText)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        var data = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(data);
    }

    public string Decrypt(string privateKey, string cipherText)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        var data = rsa.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(data);
    }
}