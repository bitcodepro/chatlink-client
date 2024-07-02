using ChatLink.Client.ViewModels;

namespace ChatLink.Client.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _loginViewModel;

    public LoginPage(LoginViewModel loginViewModel)
    {
        InitializeComponent();
        _loginViewModel = loginViewModel;
        BindingContext = loginViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, false);

        await _loginViewModel.ClearToken();
    }
}