using CryptoLab.Common;
using Xunit;

namespace CryptoLab.Tests;

public class HashServiceTests
{
    private readonly HashService _service = new();

    [Fact]
    public void ComputeHash_SameText_ReturnsSameHash()
        => Assert.Equal(_service.ComputeHash("Привіт"), _service.ComputeHash("Привіт"));

    [Fact]
    public void Verify_CorrectHash_ReturnsTrue()
    {
        var hash = _service.ComputeHash("test");
        Assert.True(_service.Verify("test", hash));
    }

    [Fact]
    public void Verify_ChangedData_ReturnsFalse()
    {
        var hash = _service.ComputeHash("test");
        Assert.False(_service.Verify("test!", hash));
    }

    [Fact]
    public void Verify_InvalidBase64_ReturnsFalse()
        => Assert.False(_service.Verify("test", "not-base64!!!"));
}