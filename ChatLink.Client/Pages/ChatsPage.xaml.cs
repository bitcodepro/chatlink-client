using ChatLink.Client.Services.Interfaces;
using ChatLink.Client.ViewModels;

namespace ChatLink.Client.Pages;

public partial class ChatsPage : ContentPage
{
    private readonly ChatsViewModel _chatsViewModel;
    private readonly IUserService _userService;

    public ChatsPage(ChatsViewModel chatsViewModel, IUserService userService)
    {
        _chatsViewModel = chatsViewModel;
        _userService = userService;
        InitializeComponent();
        BindingContext = _chatsViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CheckAuthorization();
    }

    private async void CheckAuthorization()
    {
        if (! await _userService.IsUserAuthorized())
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
    }
}