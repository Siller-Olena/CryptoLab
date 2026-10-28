namespace CryptoLab.Common;

public interface IAesService
{
    AesKey GenerateSecretKey();
    string Encrypt(AesKey aesKey, string plainText);
    string Decrypt(AesKey aesKey, string cipherText);
}