using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatLink.Client.Models;
using ChatLink.Client.Pages;
using ChatLink.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatLink.Client.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private AuthUser authUser;

    public RegistrationViewModel(IUserService userService)
    {
        _userService = userService;
        authUser = new AuthUser
        {
            Server = "http://localhost:40000",
            Email = string.Empty,
            Password = string.Empty,
            UserName = string.Empty
        };
    }

    [RelayCommand]
    private async Task Register()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Error", "No Internet connection", "OK");
        }

        bool result = await _userService.SingUp(authUser);

        if (!result)
        {
            await Shell.Current.DisplayAlert("Error", "Something went wrong", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }

    [RelayCommand]
    private async Task GoToLoginPage()
    {
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }
}
