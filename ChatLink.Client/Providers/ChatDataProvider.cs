using System.Net;
using ChatLink.Client.Models;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Models.Enums;
using ChatLink.Client.Pages;
using ChatLink.Client.Providers.Handlers;
using ChatLink.Client.Services;
using ChatLink.Client.Services.Interfaces;
using Newtonsoft.Json;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace ChatLink.Client.Providers;

public class ChatDataProvider : BaseDataProvider
{
    public static bool CanShowNotification = true;

    private int numberMessage = 1;
    private readonly IUserService _userService;
    private readonly IEncryptionService _encryptionService;
    private readonly ISessionService _sessionService;
    private readonly IApiService _apiService;
    private readonly FileService _fileService;
    private readonly ChatHandler _chatHandler;
    private readonly ChatsHandler _chatsHandler;

    public ChatDataProvider(IUserService userService, 
        IEncryptionService encryptionService, 
        ISessionService sessionService, 
        IApiService apiService,
        FileService fileService,
        ChatHandler chatHandler, 
        ChatsHandler chatsHandler)
    {
        _userService = userService;
        _encryptionService = encryptionService;
        _sessionService = sessionService;
        _apiService = apiService;
        _fileService = fileService;
        _chatHandler = chatHandler;
        _chatsHandler = chatsHandler;

        _chatHandler.OnUpdateChat += UpdateChat;
        _chatHandler.OnReceiveTheCall += ReceiveTheCall;
        _chatHandler.OnFinishCall += FinishCall;

        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
    }

    public async Task<List<MessageDto>> GetMessages(Guid sessionId)
    {
        var messagesDto = new List<MessageDto>();
        var messages = await _sessionService.GetMessages(sessionId);

        var currentUser = await _userService.GetCurrentUser();
        var userIds = messages.Select(x => x.Email).Distinct().ToList();
        var users = await _userService.GetUsers(userIds);
        users.Add(currentUser);

        foreach (var message in messages)
        {
            var u = users.Find(x => x.Email == message.Email);

            messagesDto.Add(new MessageDto
            {
                MessageId = message.Id,
                Email = message.Email,
                DecryptedMessage = message.DecryptedMessage,
                EncryptedMessage = message.EncryptedMessage,
                MessageCreatedDt = message.CreatedDt,
                SessionId = message.SessionId,
                Type = Enum.Parse<MessageType>(message.Type),
                UserName = u.UserName,
                ImageUrl = u.ImageUrl,
                IsCurrentUser = currentUser.Email == message.Email,
                IsPlay = true,
                IsPlaying = false
            });
        }

        return messagesDto;
    }

    public async Task SendMessage(string email, Guid sessionId, string message, MessageType type)
    {
        var encryptedMessage = await _encryptionService.EncryptMessage(email, message);

        await _userService.SendMessage(email, new MessageTinyDto
        {
            EncryptedMessage = encryptedMessage,
            SessionId = sessionId,
            Type = type
        });
    }

    public async Task GetMissedMessages()
    {
        try
        {
            var newMessageDtos = await _userService.GetMissedMessage();

            foreach (var mess in newMessageDtos)
            {
                if (await _sessionService.ExistsMessage(mess.MessageId))
                {
                    continue;
                }

                string decryptedMessage = mess.EncryptedMessage;

                try
                {
                    decryptedMessage = await _encryptionService.DecryptMessage(mess.Email, mess.EncryptedMessage);
                }
                catch (Exception e)
                {

                }

                if (string.IsNullOrWhiteSpace(decryptedMessage))
                {
                    continue;
                }

                mess.DecryptedMessage = decryptedMessage;
                await SaveFile(mess.Email, mess);

                await SaveMessages(mess);
            }
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound || e.StatusCode == HttpStatusCode.Unauthorized)
        {
            _onLogin?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _onDataChanged?.Invoke();
        }
    }

    private async Task SaveMessages(MessageDto mess)
    {
        await _sessionService.SaveMessage(new Message
        {
            Id = mess.MessageId,
            Email = mess.Email,
            CreatedDt = mess.MessageCreatedDt,
            DecryptedMessage = mess.DecryptedMessage,
            EncryptedMessage = mess.EncryptedMessage,
            SessionId = mess.SessionId,
            Type = mess.Type.ToString()
        });
    }

    private async void UpdateChat(string email, string encryptedMessage)
    {
        _chatHandler.needUpdateChat = false;

        try
        {
            var mess = JsonConvert.DeserializeObject<MessageDto>(encryptedMessage);

            string decryptedMessage = await _encryptionService.DecryptMessage(email, mess.EncryptedMessage);

            if (string.IsNullOrWhiteSpace(decryptedMessage))
            {
                return;
            }

            mess.DecryptedMessage = decryptedMessage;

            await SaveFile(email, mess);
            await SaveMessages(mess);

            _onDataChanged?.Invoke();

            _chatsHandler.UpdateLastMessageInChat();

            ShowLocalNotification(email, decryptedMessage, mess.SessionId);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            _onLogin?.Invoke();
        }
    }

    private async Task SaveFile(string email, MessageDto mess)
    {
        string fileName = $"{mess.SessionId}/{Guid.NewGuid()}";
        var jwtToken = await _userService.GetJwtToken();

        if (mess.Type == MessageType.Image || mess.Type == MessageType.Audio)
        {
            var encryptedFileData = await _apiService.GetBinaryData(mess.DecryptedMessage, jwtToken);
            var decryptedFileData = await _encryptionService.DecryptData(email, encryptedFileData);

            mess.DecryptedMessage = await _fileService.SaveFileLocallyAsync(decryptedFileData, fileName);
        }
        else if (mess.Type == MessageType.Video || mess.Type == MessageType.File)
        {
            var localPath = FileSystem.Current.AppDataDirectory;
            var outputFilePath = Path.Combine(localPath, fileName);

            fileName = $"{mess.SessionId}/e{Guid.NewGuid()}";
            var outputFilePath2 = Path.Combine(localPath, fileName);

            await _userService.DownloadFileAsync(mess.DecryptedMessage, outputFilePath2);
            await _encryptionService.DecryptLargeFile(email, outputFilePath2, outputFilePath);

            mess.DecryptedMessage = outputFilePath;

            if (File.Exists(outputFilePath2))
            {
                File.Delete(outputFilePath2);
            }
        }
    }

    private void ShowLocalNotification(string email,string message, Guid sessionId)
    {
        if (!CanShowNotification)
        {
            return;
        }

        var notification = new NotificationRequest
        {
            NotificationId = numberMessage++,
            Title = email,
            Description = message,
            ReturningData = sessionId.ToString(),
            //Sound = "sound.wav"
        };

        LocalNotificationCenter.Current.Show(notification);
    }

    private async void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Request.ReturningData) && e.IsTapped)
        {
            var sessionDto = await _sessionService.GetUserSessionById(Guid.Parse(e.Request.ReturningData));

            if (sessionDto is null)
            {
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(ChatPage)}", true, new Dictionary<string, object>
            {
                {"sessionDto", new SessionDto
                {
                    Id = sessionDto.Session.Id,
                    Email = sessionDto.User.Email,
                    UserName = sessionDto.User.UserName,
                    ImageUrl = "man.png",
                    Title = sessionDto.Session.Title,
                    LastMessage = "",
                    MessageCreatedDt = DateTime.UtcNow
                }}
            });
        }
    }

    private async void ReceiveTheCall(string email)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"{nameof(VideoOtherPage)}", true, new Dictionary<string, object>
            {
                {"email", email}
            });
        });
    }

    private async void FinishCall()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"//{nameof(ChatsPage)}");
        });
    }
}
