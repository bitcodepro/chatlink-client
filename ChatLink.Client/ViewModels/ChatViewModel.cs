using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using ChatLink.Client.Constants;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Models.Enums;
using ChatLink.Client.Pages;
using ChatLink.Client.Providers;
using ChatLink.Client.Services;
using ChatLink.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Plugin.Maui.Audio;

namespace ChatLink.Client.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ChatDataProvider _dataProvider;
    private readonly IUserService _userService;
    private readonly IEncryptionService _encryptionService;
    private readonly FileService _fileService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<MessageDto> items = new();

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private bool isSendVisible;

    [ObservableProperty]
    private bool isMicrophoneVisible = true;    
    
    [ObservableProperty]
    private bool isRecordingVisible;

    private MessageDto? currentMessageDto;
    private double stepProgress = 0;
    private IAudioPlayer _player;

    public IDispatcherTimer timer;

    public SessionDto SessionContext { get; set; }

    public event Action<MessageDto> ScrollToNewItem;

    public ChatViewModel(ChatDataProvider chatDataProvider, IUserService userService, IEncryptionService encryptionService, FileService fileService)
    {
        _dataProvider = chatDataProvider;
        _userService = userService;
        _encryptionService = encryptionService;
        _fileService = fileService;
        _dataProvider = chatDataProvider;

        _dataProvider.OnDataChanged += UpdateItems;
        _dataProvider.OnLogin += Login;

    }

    [RelayCommand]
    private async Task CallAudio()
    {
        await Shell.Current.GoToAsync($"{nameof(VideoPage)}", true, new Dictionary<string, object>
        {
            {"email" ,SessionContext.Email}
        });


    }

    [RelayCommand]
    private async void OpenImageOnFullscreen(MessageDto messageDto)
    {
        if (!string.IsNullOrEmpty(messageDto.DecryptedMessage))
        {
            await Application.Current.MainPage.Navigation.PushAsync(new FullScreenImagePage(messageDto.DecryptedMessage));
        }
    }

    [RelayCommand]
    private async Task PlayAudio(MessageDto messageDto)
    {
        if (!timer.IsRunning)
        {
            currentMessageDto = messageDto;

            if (!File.Exists(messageDto.DecryptedMessage))
            {
                return;
            }

            await using Stream fileStream = File.OpenRead(messageDto.DecryptedMessage);

            _player = AudioManager.Current.CreatePlayer(fileStream);
            stepProgress = 1 / _player.Duration;
            messageDto.IsPlay = false;
            messageDto.IsPlaying = true;

            _player.Play();
            timer.Start();
        }
    }

    [RelayCommand]
    private void StopAudio(MessageDto messageDto)
    {
        if (timer.IsRunning)
        {
            messageDto.IsPlaying = false;
            messageDto.IsPlay = true;

            _player.Stop();
            timer.Stop();
            messageDto.Progress = 0;
        }
    }

    public void Timer_Tick(object sender, EventArgs e)
    {
        if (timer.IsRunning && currentMessageDto.Progress + stepProgress >= 1)
        {
            timer.Stop();
            currentMessageDto.IsPlaying = false;
            currentMessageDto.IsPlay = true;
            currentMessageDto.Progress = 0;
        }
        else
        {
            currentMessageDto.Progress += stepProgress;
        }
    }

    public async Task SendMessageBinaryData(string message, MessageType type)
    {
        await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, message, type);
    }

    public async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(SessionContext.Email))
        {
            return;
        }

        await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, message, MessageType.Text);
        Message = string.Empty;
    }

    public async Task GetAllMessages()
    {
        await _dataProvider.GetMissedMessages();
    }

    public async void UploadImage()
    {
        var photo = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Pick a file",
            FileTypes = FilePickerFileType.Images
        });

        if (photo == null)
        {
            return;
        }

        var stream = await photo.OpenReadAsync();
        var fileUrl = await SendBinaryData(stream, photo.FileName);

        if (!string.IsNullOrEmpty(fileUrl))
        {
            await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, fileUrl, MessageType.Image);
        }
    }

    public async void TakePhoto()
    {
        if (!MediaPicker.IsCaptureSupported)
        {
            return;
        }

        FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

        try
        {
            if (photo is null)
            {
                return;
            }

            Stream stream = await photo.OpenReadAsync();
            var fileUrl = await SendBinaryData(stream, photo.FileName);

            if (!string.IsNullOrEmpty(fileUrl))
            {
                await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, fileUrl, MessageType.Image);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        //UserNameImage.Source = ImageSource.FromStream(() => stream);
    }

    public async void UploadVideo()
    {
        var video = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Pick a file",
            FileTypes = FilePickerFileType.Videos
        });

        if (video == null)
        {
            return;
        }

        var stream = await video.OpenReadAsync();
        var (filePath, filePath2, filePath3) = _fileService.SaveStreamToFile(stream, video.FileName);
        await _encryptionService.EncryptLargeFile(SessionContext.Email, filePath, filePath2);
        await _encryptionService.DecryptLargeFile(SessionContext.Email, filePath2, filePath3);

        using Stream inputFileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);
        var fileUrl = await _userService.SendBinaryData(inputFileStream2, video.FileName, SessionContext.Id);

        try
        {
            if (File.Exists(filePath) && File.Exists(filePath2) && File.Exists(filePath3))
            {
                File.Delete(filePath);
                File.Delete(filePath2);
                File.Delete(filePath3);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        if (!string.IsNullOrEmpty(fileUrl))
        {
            await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, fileUrl, MessageType.Video);
        }
    }

    public async void TakeVideo()
    {
        if (!MediaPicker.IsCaptureSupported)
        {
            return;
        }

        FileResult? video = await MediaPicker.Default.CaptureVideoAsync();

        try
        {
            if (video is null)
            {
                return;
            }

            await using Stream stream = await video.OpenReadAsync();
            var (filePath, filePath2, filePath3) = _fileService.SaveStreamToFile(stream, video.FileName);
            await _encryptionService.EncryptLargeFile(SessionContext.Email, filePath, filePath2);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _encryptionService.DecryptLargeFile(SessionContext.Email, filePath2, filePath3);

            using Stream inputFileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);
            var fileUrl = await _userService.SendBinaryData(inputFileStream2, video.FileName, SessionContext.Id);

            if (File.Exists(filePath2) && File.Exists(filePath3))
            {
                File.Delete(filePath2);
                File.Delete(filePath3);
            }

            if (!string.IsNullOrEmpty(fileUrl))
            {
                await _dataProvider.SendMessage(SessionContext.Email, SessionContext.Id, fileUrl, MessageType.Video);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string?> SendBinaryData(Stream stream, string fileName)
    {
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var encryptedFileData = await _encryptionService.EncryptData(SessionContext.Email, fileData);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Convert.FromBase64String(encryptedFileData));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        content.Add(fileContent, "file", fileName);

        var jwtToken = await SecureStorage.Default.GetAsync(AuthConstants.JwtToken);

        var httpClient = new HttpClient();

        if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        string url = UserService.GetUrl() + $"/api/chatlink/upload/{SessionContext.Id}";

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

    private async void UpdateItems()
    {
        var currentUser = await _userService.GetCurrentUser();

        if (currentUser is null)
        {
            return;
        }

        var messages = await _dataProvider.GetMessages(SessionContext.Id);
        var tempMessages = new List<MessageDto>();

        foreach (var messageDto in messages)
        {
            if (SessionContext.Id == messageDto.SessionId && Items.All(x => x.MessageId != messageDto.MessageId))
            {
                messageDto.IsCurrentUser = messageDto.Email == currentUser.Email;
                tempMessages.Add(messageDto);
            }
        }

        for (var i = 0; i < tempMessages.Count; i++)
        {
            var tempMessage = tempMessages[i];
            if (tempMessages.Count == i+1)
            {
                tempMessage.IsLastMessage = true;
            }

            Items.Add(tempMessage);
        }
    }

    private async void Login()
    {
        await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }
}
