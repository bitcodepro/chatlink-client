using System.Collections.ObjectModel;
using ChatLink.Client.Constants;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Models.Enums;
using ChatLink.Client.Pages;
using ChatLink.Client.Providers;
using ChatLink.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatLink.Client.ViewModels;
public partial class ChatsViewModel : ObservableObject
{
    private readonly ChatsDataProvider _dataProvider;
    private readonly ISessionService _sessionService;

    [ObservableProperty]
    private ObservableCollection<SessionDto> items = new();

    public ChatsViewModel(ChatsDataProvider chatsDataProvider, ISessionService sessionService)
    {
        _dataProvider = chatsDataProvider;
        _sessionService = sessionService;

        _dataProvider.OnDataChanged += UpdateItems;
        _dataProvider.OnLogin += Login;

        UpdateItems();
    }

    [RelayCommand]
    private async Task GoToChat(SessionDto sessionDto)
    {
        await Shell.Current.GoToAsync($"{nameof(ChatPage)}", true, new Dictionary<string, object>
        {
            {"sessionDto" ,sessionDto}
        });
    }

    private async void UpdateItems()
    {
        foreach (var userSession in await _dataProvider.GetUserSessions())
        {
            var message = await _sessionService.GetLastMessage(userSession.Session.Id);
            var item = Items.FirstOrDefault(x => x.Id == userSession.Session.Id);

            if (item is null)
            {
                var lastMessage = message is null ? string.Empty : message.DecryptedMessage;

                if (message is not null && message.Type != MessageType.Text.ToString())
                {
                    lastMessage = "New message";
                }

                items.Add(new SessionDto
                {
                    Id = userSession.Session.Id,
                    Email = userSession.User.Email,
                    UserName = userSession.User.UserName,
                    ImageUrl = "man.png",
                    Title = userSession.Session.Title,
                    LastMessage = lastMessage,
                    MessageCreatedDt = message?.CreatedDt ?? DateTime.Now,
                });
            }
            else
            {
                item.LastMessage = message is null ? string.Empty : message.DecryptedMessage;
            }
        }
    }

    private async void Login()
    {
        await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }
}
