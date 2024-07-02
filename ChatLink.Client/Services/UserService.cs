using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using ChatLink.Client.Constants;
using ChatLink.Client.Models;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatLink.Client.Services;

public class UserService : IUserService
{
    private User? _CurentUser;
    private readonly IApiService _apiService;
    private readonly ISignalRService _signalRService;
    private readonly DbLocalService _dbLocalService;
    private readonly EncryptionManager _encryptionManager;

    public UserService(IApiService apiService, ISignalRService signalRService, DbLocalService dbLocalService, IEncryptionService encryptionService, EncryptionManager encryptionManager)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        _dbLocalService = dbLocalService;
        _encryptionManager = encryptionManager;
    }

    public async Task CallAsync(string caller)
    {
    }


    #region User

    public void ResetCurrentUser()
    {
        _CurentUser = null;
    }

    public async Task<List<User>> GetUsers(List<string> ids)
    {
        return await _dbLocalService.GetUsers(ids);
    }

    public async Task<string?> GetJwtToken()
    {
        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        return jwtToken;
    }

    public async Task<bool> IsUserAuthorized()
    {
        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        return jwtToken is not null && !string.IsNullOrWhiteSpace(jwtToken);
    }

    public async Task<string?> SingIn(AuthUser authUser)
    {
        var parameters = new Dictionary<string, string>
        {
            { "email", authUser.Email },
            { "password", authUser.Password }
        };

        var url = GetUrl() + "/api/chatlink/Auth/login";

        try
        {
            var jwtToken = await _apiService.PostAsync<Dictionary<string, string>, JwtDto>(url, parameters);

            if (jwtToken is null)
            {
                return null;
            }

            await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, jwtToken.AccessToken);

            _CurentUser = await GetCurrentUser();

            await SecureStorage.Default.SetAsync(DataProviderConstants.DataCurrentUser, JsonConvert.SerializeObject(_CurentUser));

            return jwtToken?.AccessToken;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<bool> SingUp(AuthUser authUser)
    {
        var parameters = new Dictionary<string, string>
        {
            { "email", authUser.Email },
            { "password", authUser.Password },
            { "userName", authUser.UserName }
        };

        var url = GetUrl() + "/api/chatlink/Auth/register";

        try
        {
            await _apiService.PostAsync(url, parameters);

            return true;

        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<User?> FindUserByEmail(string email)
    {
        var url = GetUrl() + "/api/chatlink/Search/get-user";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var headers = new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {jwtToken}"}
        };

        var user = await _apiService.PostAsync<string, User?>(url, email, headers);

        return user;
    }

    public async Task<User?> GetCurrentUser()
    {
        if (_CurentUser is not null)
        {
            return _CurentUser;
        }

        var jsonUser = await SecureStorage.Default.GetAsync(DataProviderConstants.DataCurrentUser);

        if (jsonUser != null && !string.IsNullOrWhiteSpace(jsonUser))
        {
            _CurentUser = JsonConvert.DeserializeObject<User>(jsonUser);

            return _CurentUser;
        }

        var url = GetUrl() + "/api/chatlink/Search/get-current-user";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var headers = new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {jwtToken}"}
        };

        try
        {
            var user = await _apiService.PostAsync<string, User?>(url, "", headers);
            _CurentUser = user;

            return user;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<UserSession>> GetUserSession()
    {
        var url = GetUrl() + "/api/chatlink/Search/get-user-session-data";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var headers = new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {jwtToken}"}
        };

        var users = await _apiService.PostAsync<string, List<UserSession>>(url, "", headers);

        return users;
    }

    public async Task<List<MessageDto>> GetMissedMessage()
    {
        var url = GetUrl() + "/api/chatlink/Search/get-missed-messages";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var headers = new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {jwtToken}"}
        };

        var messageDtos = await _apiService.PostAsync<string, List<MessageDto>>(url, "", headers);

        return messageDtos;
    }

    public async Task<List<User>> GetContacts()
    {
        var url = GetUrl() + "/api/chatlink/Search/get-contacts";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var headers = new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {jwtToken}"}
        };

        var users = await _apiService.PostAsync<string, List<User>>(url, "", headers);

        return users;
    }

    #endregion User

    #region SignalR

    public async Task StartSignalR()
    {
        await ConnectSignalR();
    }

    public async Task StopSignalR()
    {
        await _signalRService.Stop();
    }

    public async Task ConnectToUser(string email)
    {
        var ecdh = _encryptionManager.GetEcdhsById(email) ?? _encryptionManager.Create(email);
        string publicKey = _encryptionManager.GetPublicKey(email);

        await _signalRService.Send(SignalRConstants.SendPublicKey, email, publicKey);
    }

    public async Task SendMessage(string email, MessageTinyDto messageTinyDto)
    {
        await _signalRService.Send(SignalRConstants.SendMessage, email, messageTinyDto);
    }

    private async Task ConnectSignalR()
    {
        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        if (jwtToken is null)
        {
            return;
        }

        await _signalRService.Start(jwtToken, ReceivePublicKey);
    }

    private async void ReceivePublicKey(string email, string publicKey)
    {
        var otherPublicKey = ECDiffieHellman.Create();
        otherPublicKey.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

        bool isSendMyPublic = false;
        var ecdh = _encryptionManager.GetEcdhsById(email);
        if (ecdh is null)
        {
            ecdh = _encryptionManager.Create(email);
            isSendMyPublic = true;
        }

        var sharedKey = ecdh.DeriveKeyMaterial(otherPublicKey.PublicKey);

        _encryptionManager.SetSharedKeyById(email, sharedKey);

        if (isSendMyPublic)
        {
            await ConnectToUser(email);
        }

        await _signalRService.Send(SignalRConstants.CreateChat, email);
    }

    #endregion SignarR

    #region Sending files

    public async Task<string?> SendBinaryData(Stream stream, string fileName, Guid sessionId)
    {
        using var httpClient = new HttpClient();
        string url = UserService.GetUrl() + $"/api/chatlink/upload/{sessionId}";

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        content.Add(streamContent, "file", fileName);

        if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        var response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            var fileUrl = jsonResponse["fileUrl"]!.ToString();

            return fileUrl;
        }

        return null;
    }

    public async Task DownloadFileAsync(string url, string filePath)
    {
        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        using HttpClient httpClient = new HttpClient();

        if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        using (HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                   fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }
        }
    }

    #endregion Sending files

    public static string GetUrl()
    {
        string url = "http://localhost:40000";

#if __ANDROID__

        
        url = SignalRService.GetUrl();
#endif

        return url;
    }
}
