using System.Security.Cryptography;
using ChatLink.Client.Constants;

namespace ChatLink.Client.Models;

public class EncryptionManager
{
    private Dictionary<string, ECDiffieHellman> ecdhs = new();

    public ECDiffieHellman Create(string id)
    {
        ECDiffieHellman ecdh = ECDiffieHellman.Create();

        if (ecdhs.TryGetValue(id, out ECDiffieHellman existingValue))
        {
            ecdhs[id] = ecdh;
        }
        else
        {
            ecdhs.Add(id, ecdh);
        }

        return ecdh;
    }

    public string GetPublicKey(string email)
    {
        var ecdh = GetEcdhsById(email);

        if (ecdh is null)
        {
            throw new InvalidOperationException("Can't get public key");
        }

        string publicKey = Convert.ToBase64String(ecdh.PublicKey.ExportSubjectPublicKeyInfo());

        return publicKey;
    }

    public async Task SetSharedKeyById(string id, byte[]? sharedKey)
    {
        if (sharedKey is null)
        {
            return;
        }

        string sharedId = GetId(id);

        string sharedKeyStr = Convert.ToBase64String(sharedKey);

        await SecureStorage.Default.SetAsync(sharedId, sharedKeyStr);
    }

    public async Task<byte[]?> GetSharedKeyById(string id)
    {
        string sharedId = GetId(id);
        var sharedKeyStr = await SecureStorage.Default.GetAsync(sharedId);

        if (string.IsNullOrWhiteSpace(sharedKeyStr))
        {
            return [];
        }

        byte[] sharedKey = Convert.FromBase64String(sharedKeyStr);

        return sharedKey;
    }

    public ECDiffieHellman? GetEcdhsById(string id)
    {
        return ecdhs.GetValueOrDefault(id);
    }

    private string GetId(string id)
    {
        return DataProviderConstants.DataSharedKey + id;
    }
}
