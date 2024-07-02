using System.Collections.ObjectModel;
using System.Net;
using ChatLink.Client.Constants;
using ChatLink.Client.Models;
using ChatLink.Client.Pages;
using ChatLink.Client.Providers;
using ChatLink.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatLink.Client.ViewModels;
public partial class ContactsViewModel : ObservableObject
{
    private readonly ContactDataProvider _dataProvider;

    [ObservableProperty]
    private ObservableCollection<User> items = new();

    [ObservableProperty]
    private ObservableCollection<User> foundUsers = new();

    [ObservableProperty]
    private string searchEmail = string.Empty;

    public ContactsViewModel(ContactDataProvider dataProvider)
    {
        _dataProvider = dataProvider;

        _dataProvider.OnDataChanged += UpdateItems;
        _dataProvider.OnLogin += Login;

        UpdateItems();
    }

    [RelayCommand]
    private async Task Remove(User user)
    {
        await _dataProvider.DeleteUser(user);
    }


    [RelayCommand]
    private async Task Search()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchEmail))
            {
                await Shell.Current.DisplayAlert("Error", "Enter email", "OK");

                return;
            }

            foundUsers.Clear();

            var user = await _dataProvider.Search(searchEmail);

            if (user is null)
            {
                 return;
            }

            foundUsers.Add(user);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
        catch (Exception e)
        {
            await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
    }

    public async Task Connect()
    {
        var user = foundUsers.First();
        await _dataProvider.ConnectToUser(user.Email);
    }

    private async void UpdateItems()
    {
        items.Clear();

        foreach (var user in await _dataProvider.GetUsers())
        {
            items.Add(user);
        }
    }

    private async void Login()
    {
        await SecureStorage.Default.SetAsync(AuthConstants.JwtToken, "");
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }
}
