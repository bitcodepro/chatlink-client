using ChatLink.Client.Models;
using ChatLink.Client.Services.Interfaces;
using ChatLink.Services;

namespace ChatLink.Client.Services;

public class EncryptionService : IEncryptionService
{
    private readonly EncryptionManager _encryptionManager;

    public EncryptionService(EncryptionManager encryptionManager)
    {
        _encryptionManager = encryptionManager;
    }

    public async Task<string> EncryptMessage(string id, string message)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return string.Empty;
        }

        string combinedDataBase64 = AesGcmEncryptionService.EncryptString(sharedKey, message.Trim());

        return combinedDataBase64;
    }

    public async Task<string> DecryptMessage(string id, string combinedDataBase64)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return string.Empty;
        }

        string decryptedMessage = AesGcmEncryptionService.DecryptString(sharedKey, combinedDataBase64);

        return decryptedMessage;
    }

    public async Task<string> EncryptData(string id, byte[] fileData)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return string.Empty;
        }

        var encryptedFileData = AesGcmEncryptionService.EncryptFile(sharedKey, fileData);

        return encryptedFileData;
    }

    public async Task<byte[]> DecryptData(string id, byte[] combinedData)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return [];
        }

        var decryptedFileData = AesGcmEncryptionService.DecryptFile(sharedKey, combinedData);

        return decryptedFileData;
    }

    public async Task EncryptLargeFile(string id, string inputFilePath, string outputFilePath)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return;
        }

        AesGcmEncryptionService.EncryptLargeFile(inputFilePath, outputFilePath, sharedKey);
    }

    public async Task DecryptLargeFile(string id, string inputFilePath, string outputFilePath)
    {
        var sharedKey = await _encryptionManager.GetSharedKeyById(id);

        if (sharedKey is null)
        {
            return;
        }

        AesGcmEncryptionService.DecryptLargeFile(inputFilePath, outputFilePath, sharedKey);
    }
}
