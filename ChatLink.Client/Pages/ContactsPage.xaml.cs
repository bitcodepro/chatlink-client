using ChatLink.Client.Services.Interfaces;
using ChatLink.Client.ViewModels;

namespace ChatLink.Client.Pages;

public partial class ContactsPage : ContentPage
{
    private readonly IUserService _userService;

    public ContactsPage(ContactsViewModel contactsViewModel, IUserService userService)
	{
        _userService = userService;
        InitializeComponent();
        BindingContext = contactsViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CheckAuthorization();
    }

    private async void CheckAuthorization()
    {
        //await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");

        if (! await _userService.IsUserAuthorized())
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
    }

    private async void OnConnectBtn(object? sender, EventArgs e)
    {
        var viewModel = BindingContext as ContactsViewModel;

        await viewModel.Connect();
    }

    private async void OnRemoveBtn(object? sender, EventArgs e)
    {
        var viewModel = BindingContext as ContactsViewModel;

        await viewModel.Connect();
    }
}