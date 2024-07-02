using System.Security.Cryptography;
using System.Text;

namespace ChatLink.Services;

public static class AesGcmEncryptionService
{
    private static readonly int NonceSize = 12; // 96 bits for the nonce is standard
    private static readonly int TagSize = 16;   // 128 bits for the tag is standard
    private const int BufferSize = 1024 * 4;

    public static (byte[] encryptedData, byte[] nonce, byte[] tag) EncryptData(byte[] data, byte[] key)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] encryptedData = new byte[data.Length];
            byte[] tag = new byte[TagSize];

            aesGcm.Encrypt(nonce, data, encryptedData, tag);

            return (encryptedData, nonce, tag);
        }
    }

    public static void EncryptLargeFile(string inputFilePath, string outputFilePath, byte[] key)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // Write the nonce to the output file
                outputFileStream.Write(nonce, 0, NonceSize);

                byte[] buffer = new byte[BufferSize];
                byte[] encryptedBuffer = new byte[BufferSize];
                byte[] tag = new byte[TagSize];

                int bytesRead;
                while ((bytesRead = inputFileStream.Read(buffer, 0, BufferSize)) > 0)
                {
                    // Adjust buffer size for the last block
                    if (bytesRead < BufferSize)
                    {
                        Array.Resize(ref encryptedBuffer, bytesRead);
                    }

                    // Encrypt the data
                    aesGcm.Encrypt(nonce, buffer.AsSpan(0, bytesRead), encryptedBuffer, tag);

                    // Write encrypted data to the output file
                    outputFileStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
                    outputFileStream.Write(tag, 0, TagSize);

                    // Flush the data to disk after each write
                    outputFileStream.Flush();
                }
            }
        }
    }


    public static void DecryptLargeFile(string inputFilePath, string outputFilePath, byte[] key)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] nonce = new byte[NonceSize];

            using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // Read the nonce from the beginning of the encrypted file
                inputFileStream.Read(nonce, 0, NonceSize);

                byte[] buffer = new byte[BufferSize + TagSize];
                byte[] decryptedBuffer = new byte[BufferSize];
                byte[] tag = new byte[TagSize];

                int bytesRead;
                while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead <= TagSize)
                    {
                        throw new InvalidOperationException("File is corrupted or the buffer size is too small.");
                    }

                    // Separate encrypted data and tag
                    int encryptedDataSize = bytesRead - TagSize;
                    Array.Copy(buffer, encryptedDataSize, tag, 0, TagSize);

                    // Adjust buffer size for the last block
                    if (encryptedDataSize < BufferSize)
                    {
                        decryptedBuffer = new byte[encryptedDataSize];
                    }

                    aesGcm.Decrypt(nonce, buffer.AsSpan(0, encryptedDataSize), tag, decryptedBuffer);

                    // Write decrypted data to the output file
                    outputFileStream.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                    outputFileStream.Flush();
                }
            }
        }
    }

    public static byte[] DecryptData(byte[] encryptedData, byte[] key, byte[] nonce, byte[] tag)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] decryptedData = new byte[encryptedData.Length];
            aesGcm.Decrypt(nonce, encryptedData, tag, decryptedData);

            return decryptedData;
        }
    }

    public static string EncryptString(byte[] key, string plainText)
    {
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        var (encryptedData, nonce, tag) = EncryptData(data, key);

        byte[] combinedData = new byte[nonce.Length + tag.Length + encryptedData.Length];
        Array.Copy(nonce, 0, combinedData, 0, nonce.Length);
        Array.Copy(tag, 0, combinedData, nonce.Length, tag.Length);
        Array.Copy(encryptedData, 0, combinedData, nonce.Length + tag.Length, encryptedData.Length);

        return Convert.ToBase64String(combinedData);
    }

    public static string DecryptString(byte[] key, string encryptedText)
    {
        byte[] combinedData = Convert.FromBase64String(encryptedText);

        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] encryptedData = new byte[combinedData.Length - NonceSize - TagSize];

        Array.Copy(combinedData, 0, nonce, 0, NonceSize);
        Array.Copy(combinedData, NonceSize, tag, 0, TagSize);
        Array.Copy(combinedData, NonceSize + TagSize, encryptedData, 0, encryptedData.Length);

        byte[] decryptedData = DecryptData(encryptedData, key, nonce, tag);

        return Encoding.UTF8.GetString(decryptedData);
    }

    public static string EncryptFile(byte[] key, byte[] fileData)
    {
        var (encryptedData, nonce, tag) = EncryptData(fileData, key);

        byte[] combinedData = new byte[nonce.Length + tag.Length + encryptedData.Length];
        Array.Copy(nonce, 0, combinedData, 0, nonce.Length);
        Array.Copy(tag, 0, combinedData, nonce.Length, tag.Length);
        Array.Copy(encryptedData, 0, combinedData, nonce.Length + tag.Length, encryptedData.Length);

        return Convert.ToBase64String(combinedData);
    }

    public static byte[] DecryptFile(byte[] key, byte[] combinedData)
    {
        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] encryptedData = new byte[combinedData.Length - NonceSize - TagSize];

        Array.Copy(combinedData, 0, nonce, 0, NonceSize);
        Array.Copy(combinedData, NonceSize, tag, 0, TagSize);
        Array.Copy(combinedData, NonceSize + TagSize, encryptedData, 0, encryptedData.Length);

        return DecryptData(encryptedData, key, nonce, tag);
    }
}
