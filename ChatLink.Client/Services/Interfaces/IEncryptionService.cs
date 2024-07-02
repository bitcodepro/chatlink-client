namespace ChatLink.Client.Services.Interfaces;

public interface IEncryptionService
{
    public Task<string> EncryptMessage(string id, string message);
    public Task<string> DecryptMessage(string id, string combinedDataBase64);

    public Task<string> EncryptData(string id, byte[] fileData);
    public Task<byte[]> DecryptData(string id, byte[] combinedData);

    public Task EncryptLargeFile(string id, string inputFilePath, string outputFilePath);
    public Task DecryptLargeFile(string id, string inputFilePath, string outputFilePath);

    // public Task EncryptStream(string id, Stream inputStream, Stream encryptedStream);
    //
    // public Task DecryptStream(string id, Stream encryptedStream, Stream outputStream);
}
