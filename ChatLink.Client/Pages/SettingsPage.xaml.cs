using ChatLink.Client.Services.Interfaces;

namespace ChatLink.Client.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly IUserService _userService;

    public SettingsPage(IUserService userService)
    {
        _userService = userService;
        InitializeComponent();
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