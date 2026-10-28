namespace CryptoLab.Common;

public interface IRsaService
{
    RsaKeys GenerateCryptoKeys();
    string Encrypt(string publicKey, string plainText);
    string Decrypt(string privateKey, string cipherText);
}