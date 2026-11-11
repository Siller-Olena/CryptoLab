using CryptoLab.Common;
using CryptoLab.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoLab.Api;

public class KeyStorage
{
    private readonly AppDbContext _db;

    public KeyStorage(AppDbContext db) => _db = db;

    public int Add(RsaKeys keys)
    {
        var entity = new RsaKeyEntity
        {
            Name = keys.Name ?? string.Empty,
            PublicKey = keys.PublicKey,
            PrivateKey = keys.PrivateKey
        };
        _db.RsaKeys.Add(entity);
        _db.SaveChanges();
        return entity.Id;
    }

    public RsaKeys? Get(int id)
    {
        var e = _db.RsaKeys.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return e is null ? null : ToModel(e);
    }

    public IReadOnlyList<(int Id, RsaKeys Keys)> List() =>
        _db.RsaKeys.AsNoTracking().OrderBy(x => x.Id).ToList()
            .Select(e => (e.Id, ToModel(e))).ToList();

    public bool Rename(int id, string name)
    {
        var e = _db.RsaKeys.FirstOrDefault(x => x.Id == id);
        if (e is null) return false;
        e.Name = name;
        _db.SaveChanges();
        return true;
    }

    public bool Remove(int id)
    {
        var e = _db.RsaKeys.FirstOrDefault(x => x.Id == id);
        if (e is null) return false;
        _db.RsaKeys.Remove(e);
        _db.SaveChanges();
        return true;
    }

    public (int Id, RsaKeys Keys)? GetRandom()
    {
        var ids = _db.RsaKeys.AsNoTracking().Select(x => x.Id).ToList();
        if (ids.Count == 0) return null;
        var id = ids[Random.Shared.Next(ids.Count)];
        var keys = Get(id);
        return keys is null ? null : (id, keys);
    }

    private static RsaKeys ToModel(RsaKeyEntity e) => new()
    {
        Name = e.Name,
        PublicKey = e.PublicKey,
        PrivateKey = e.PrivateKey
    };
}