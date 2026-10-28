using CryptoLab.Common;
using Xunit;

namespace CryptoLab.Tests;

public class RsaServiceTests
{
    private readonly RsaService _service = new();

    [Fact]
    public void EncryptDecrypt_ReturnsOriginalText()
    {
        var keys = _service.GenerateCryptoKeys();
        var cipher = _service.Encrypt(keys.PublicKey, "Hello RSA");
        Assert.NotEqual("Hello RSA", cipher);
        Assert.Equal("Hello RSA", _service.Decrypt(keys.PrivateKey, cipher));
    }

    [Fact]
    public void EncryptDecrypt_WorksWithUkrainianText()
    {
        var keys = _service.GenerateCryptoKeys();
        var cipher = _service.Encrypt(keys.PublicKey, "Привіт, світе!");
        Assert.Equal("Привіт, світе!", _service.Decrypt(keys.PrivateKey, cipher));
    }
}