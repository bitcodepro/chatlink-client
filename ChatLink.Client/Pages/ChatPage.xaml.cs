using ChatLink.Client.CustomControls;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Models.Enums;
using ChatLink.Client.Providers;
using ChatLink.Client.Services.Interfaces;
using ChatLink.Client.ViewModels;
using CommunityToolkit.Maui.Core.Platform;
using Plugin.Maui.Audio;

namespace ChatLink.Client.Pages;

[QueryProperty("sessionDto", "sessionDto")]
public partial class ChatPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly IAudioManager _audioManager;
    private readonly IAudioRecorder _audioRecorder;
    private ChatViewModel _chatViewModel;
    private IDispatcherTimer _timer;
    public SessionDto sessionDto { get; set; }

	public ChatPage(ChatViewModel chatViewModel, IUserService userService, IAudioManager audioManager)
    {
        _userService = userService;
        _audioManager = audioManager;
        _chatViewModel = chatViewModel;

        InitializeComponent();
        BindingContext = _chatViewModel;

        _chatViewModel.ScrollToNewItem += ScrollDown;

        _chatViewModel.Items.CollectionChanged += Items_CollectionChanged;

        _audioRecorder = _audioManager.CreateRecorder();

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _chatViewModel.timer = _timer;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ChatDataProvider.CanShowNotification = false;

        CheckAuthorization();
        _chatViewModel.SessionContext = sessionDto;

        Shell.SetTabBarIsVisible(this, false);

        GetMessages();

        _timer.Tick += _chatViewModel.Timer_Tick;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ChatDataProvider.CanShowNotification = true;

        _timer.Stop();
        _timer.Tick -= _chatViewModel.Timer_Tick;
    }

    private async void GetMessages()
    {
        await _chatViewModel.GetAllMessages();
    }

    private async void CheckAuthorization()
    {
        if (! await _userService.IsUserAuthorized())
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            _chatViewModel.IsMicrophoneVisible = true;
            _chatViewModel.IsSendVisible = false;
            _chatViewModel.IsRecordingVisible = false;
        }
        else
        {
            _chatViewModel.IsRecordingVisible = false;
            _chatViewModel.IsMicrophoneVisible = false;
            _chatViewModel.IsSendVisible = true;
        }
    }

    private async void ImageButton_OnClicked(object? sender, EventArgs e)
    {
        await KeyboardExtensions.HideKeyboardAsync(ExpandingEditor);

        await _chatViewModel.SendMessage();
    }

    private void ScrollDown(MessageDto messageDto)
    {
        MessagesView.ScrollTo(messageDto, position: ScrollToPosition.End, animate: true);
    }

    private async void GoBack(object? sender, EventArgs e)
    {
        if (!await _userService.IsUserAuthorized())
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }

        await Shell.Current.GoToAsync("..", true);
    }

    private async void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: System.Collections.Specialized.NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            var newItem = (MessageDto)e.NewItems[0]!;

            if (newItem.IsLastMessage)
            {
                await Task.Delay(100);
                MessagesView.ScrollTo(newItem, position: ScrollToPosition.End, animate: true);
            }
        }
    }

    private async void MicrophoneButton_OnClicked(object? sender, EventArgs e)
    {
        if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
        {
            await Shell.Current.DisplayAlert("Error", "Add microphone permission", "OK");
            return;
        }

        if (!_audioRecorder.IsRecording)
        {
            _chatViewModel.IsMicrophoneVisible = false;
            _chatViewModel.IsSendVisible = false;
            _chatViewModel.IsRecordingVisible = true;
            await _audioRecorder.StartAsync();
        }
        else
        {
            _chatViewModel.IsSendVisible = false;
            _chatViewModel.IsRecordingVisible = false;
            _chatViewModel.IsMicrophoneVisible = true;

            var recordedAudio = await _audioRecorder.StopAsync();
            var data = recordedAudio.GetAudioStream();

            var fileUrl = await _chatViewModel.SendBinaryData(data, $"audio{Guid.NewGuid().ToString()}");

            await _chatViewModel.SendMessageBinaryData(fileUrl, MessageType.Audio);
        }
    }   
    
    private async void OpenBottomMenu_OnClicked(object? sender, EventArgs e)
    {
        var page = new CustomBottomSheet(_chatViewModel.UploadImage, _chatViewModel.TakePhoto, _chatViewModel.UploadVideo, _chatViewModel.TakeVideo);
        page.HasBackdrop = true;


        await page.ShowAsync();
    }
}