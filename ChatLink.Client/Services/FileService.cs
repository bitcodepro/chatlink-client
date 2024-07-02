namespace ChatLink.Client.Services;

public class FileService
{
    public async Task<string> SaveFileLocallyAsync(byte[] dataBytes, string fileName)
    {
        var localPath = FileSystem.Current.AppDataDirectory;

        var filePath = Path.Combine(localPath, fileName);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await File.WriteAllBytesAsync(filePath, dataBytes);

        return filePath;
    }

    public (string, string, string) SaveStreamToFile(Stream inputStream, string fileName)
    {
        var localPath = FileSystem.Current.AppDataDirectory;
        var outputFilePath = Path.Combine(localPath, fileName);
        var outputFilePath2 = Path.Combine(localPath, "en-" + fileName);
        var outputFilePath3 = Path.Combine(localPath, "de-" + fileName);

        var directoryPath = Path.GetDirectoryName(outputFilePath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        inputStream.Position = 0;

        using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        {
            inputStream.CopyTo(fileStream);
        }

        return (outputFilePath, outputFilePath2, outputFilePath3);
    }
}
