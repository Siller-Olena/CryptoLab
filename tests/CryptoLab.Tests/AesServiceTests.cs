using CryptoLab.Common;
using Xunit;

namespace CryptoLab.Tests;

public class AesServiceTests
{
    private readonly AesService _service = new();

    [Fact]
    public void EncryptDecrypt_ReturnsOriginalText()
    {
        var key = _service.GenerateSecretKey();
        var cipher = _service.Encrypt(key, "Hello AES");
        Assert.NotEqual("Hello AES", cipher);
        Assert.Equal("Hello AES", _service.Decrypt(key, cipher));
    }

    [Fact]
    public void EncryptDecrypt_WorksWithLongText()
    {
        var key = _service.GenerateSecretKey();
        var text = new string('ы', 5000);
        Assert.Equal(text, _service.Decrypt(key, _service.Encrypt(key, text)));
    }
}