using System.Security.Cryptography;
using System.Text;

namespace CryptoLab.Common;

public class AesService : IAesService
{
    public AesKey GenerateSecretKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();
        return new AesKey
        {
            Key = Convert.ToBase64String(aes.Key),
            IV = Convert.ToBase64String(aes.IV)
        };
    }

    public string Encrypt(AesKey aesKey, string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(aesKey.Key);
        aes.IV = Convert.FromBase64String(aesKey.IV);
        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public string Decrypt(AesKey aesKey, string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(aesKey.Key);
        aes.IV = Convert.FromBase64String(aesKey.IV);
        using var decryptor = aes.CreateDecryptor();
        var bytes = Convert.FromBase64String(cipherText);
        return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }
}