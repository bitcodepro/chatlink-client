using System.Net;
using ChatLink.Client.Models;
using ChatLink.Client.Providers.Handlers;
using ChatLink.Client.Services;
using ChatLink.Client.Services.Interfaces;

namespace ChatLink.Client.Providers;

public class ContactDataProvider : BaseDataProvider
{
    private readonly IUserService _userService;
    private readonly DbLocalService _dbLocalService;
    private readonly ContactHandler _contactHandler;

    public ContactDataProvider(IUserService userService, DbLocalService dbLocalService, ContactHandler contactHandler)
    {
        _userService = userService;
        _dbLocalService = dbLocalService;
        _contactHandler = contactHandler;

        _contactHandler.OnUpdateContacts += UpdateContacts;
    }

    public async Task<List<User>> GetUsers()
    {
        var users = await _dbLocalService.GetUsers();

        return users;
    }

    private async Task SaveUsers(List<User> users)
    {
        foreach (var user in users)
        {
            var u = await _dbLocalService.GetUserById(user.Email);
            if (u is null)
            {
                await _dbLocalService.CreateUser(user);
            }
        } 

        _onDataChanged?.Invoke();
    }

    public async Task DeleteUser(User user)
    {
        await _dbLocalService.DeleteUser(user);

        _onDataChanged?.Invoke();
    }

    private async void UpdateContacts()
    {
        try
        {
            var newData = await _userService.GetContacts();

            await SaveUsers(newData);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            _onLogin?.Invoke();
        }
    }

    public async Task<User?> Search(string email)
    {
        var user = await _userService.FindUserByEmail(email);

        return user;
    }

    public async Task ConnectToUser(string email)
    {
        await _userService.ConnectToUser(email);
    }
}
