using ChatLink.Client.Models;
using ChatLink.Client.Models.Dtos;

namespace ChatLink.Client.Services.Interfaces;

public interface IUserService
{
    public void ResetCurrentUser();

    public Task<bool> IsUserAuthorized();

    public Task<string?> SingIn(AuthUser authUser);
    public Task<bool> SingUp(AuthUser authUser);

    public Task<List<User>> GetUsers(List<string> ids);

    public Task<User?> FindUserByEmail(string email);

    public Task<User?> GetCurrentUser();

    public Task<List<User>> GetContacts();
    public Task<List<UserSession>> GetUserSession();

    public Task StartSignalR();
    public Task StopSignalR();

    public Task ConnectToUser(string email);

    public Task SendMessage(string email, MessageTinyDto messageTinyDto);

    public Task<List<MessageDto>> GetMissedMessage();

    // public Task<string?> SendEncryptedFile(Stream inputStream, string fileName, Guid sessionId, string id);
    //
    // public Task DownloadAndDecryptFile(string fileUrl, string outputFilePath, string id);

    public Task<string?> SendBinaryData(Stream stream, string fileName, Guid sessionId);

    public Task DownloadFileAsync(string url, string filePath);

    public Task<string?> GetJwtToken();

    public Task CallAsync(string caller);
}
