namespace CryptoLab.Common;

// Повідомлення з хешем: шифрується AES цілком
public record SecureMessage(string Data, string Hash);