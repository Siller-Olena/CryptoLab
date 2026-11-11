namespace CryptoLab.Data;

public class RsaKeyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}