namespace CryptoLab.Common;

public class RsaKeys
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}

public class AesKey
{
    public string Key { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
}

// Дані, які клієнт шифрує RSA-ключем і відправляє для створення сесії
public class SessionInitPayload
{
    public string SessionId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
}

// Обгортка для зашифрованих даних у HTTP-запитах/відповідях
public record EncryptedRequest(string Data);