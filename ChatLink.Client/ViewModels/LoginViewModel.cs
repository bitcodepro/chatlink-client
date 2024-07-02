using ChatLink.Client.Constants;
using ChatLink.Client.Models;
using ChatLink.Client.Models.Enums;
using ChatLink.Client.Pages;
using ChatLink.Client.Services;
using ChatLink.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;

namespace ChatLink.Client.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly DbLocalService _dbLocalService;

    [ObservableProperty]
    private AuthUser authUser;

    public LoginViewModel(IUserService userService, DbLocalService dbLocalService)
    {
        _userService = userService;
        _dbLocalService = dbLocalService;
        authUser = new AuthUser
        {
            Server = "https://domain.com",
            Email = string.Empty,
            Password = string.Empty,
            UserName = string.Empty
        };
    }

    [RelayCommand]
    private async Task Login()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Error", "No Internet connection", "OK");

            return;
        }

        var jwtToken = await _userService.SingIn(authUser);

        if (jwtToken is null)
        {
            await Shell.Current.DisplayAlert("Error", "Incorrect password or login", "OK");
            return;
        }

        await _userService.StartSignalR();

        await Shell.Current.Navigation.PopToRootAsync();
        await Shell.Current.GoToAsync($"///{nameof(ChatsPage)}");
    }

    [RelayCommand]
    private async Task GoToRegistrationPage()
    {
        await Shell.Current.GoToAsync($"{nameof(RegistrationPage)}");
    }

    public async Task ClearToken()
    {
        await _userService.StopSignalR();

        await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, string.Empty);
        await SecureStorage.Default.SetAsync(DataProviderConstants.DataCurrentUser, string.Empty);

        _userService.ResetCurrentUser();

        var messages = await _dbLocalService.GetAllMessages();

        foreach (var message in messages)
        {
            if (message.Type == MessageType.Audio.ToString() || message.Type == MessageType.File.ToString() || message.Type == MessageType.Image.ToString() || message.Type == MessageType.Video.ToString())
            {
                try
                {

                    if (!string.IsNullOrWhiteSpace(message.DecryptedMessage) && File.Exists(message.DecryptedMessage))
                    {
                        File.Delete(message.DecryptedMessage);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        await _dbLocalService.DeleteUsers();
        await _dbLocalService.DeleteSessions();
        await _dbLocalService.DeleteMessages();
    }
}
