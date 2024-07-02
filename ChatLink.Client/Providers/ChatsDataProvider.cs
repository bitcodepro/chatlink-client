using System.Net;
using ChatLink.Client.Models;
using ChatLink.Client.Providers.Handlers;
using ChatLink.Client.Services.Interfaces;

namespace ChatLink.Client.Providers;

public class ChatsDataProvider : BaseDataProvider
{
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;
    private readonly ChatsHandler _chatsHandler;

    public ChatsDataProvider(IUserService userService, ISessionService sessionService, ChatsHandler chatsHandler)
    {
        _userService = userService;
        _sessionService = sessionService;
        _chatsHandler = chatsHandler;

        _chatsHandler.OnUpdateChats += UpdateGetUserSession;
        _chatsHandler.OnUpdateLastMessageInChat += UpdateSessions;

        if (chatsHandler.needUpdateChats)
        {
            chatsHandler.needUpdateChats = false;
            UpdateGetUserSession();
        }
    }

    public async Task<List<UserSession>> GetUserSessions()
    {
        return await _sessionService.GetUserSessions();
    }

    private void UpdateSessions()
    {
        _onDataChanged?.Invoke();
    } 

    public async void UpdateGetUserSession()
    {
        try
        {
            List<UserSession> userSessions = await _userService.GetUserSession();

            foreach (var userSession in await GetUserSessions())
            {
                var s = userSessions.FirstOrDefault(x => x.Session.Id == userSession.Session.Id);
                if (s is null)
                {
                    await _sessionService.DeleteSession(userSession.Session);
                }
            }

            await _sessionService.SaveUserSession(userSessions);

            _onDataChanged?.Invoke();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
        {
            _onLogin?.Invoke();
        }
    }
}
