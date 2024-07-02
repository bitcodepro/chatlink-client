using ChatLink.Client.Models;
using ChatLink.Client.Services.Interfaces;

namespace ChatLink.Client.Services;

public class SessionService : ISessionService
{
    private readonly DbLocalService _dbLocalService;

    public SessionService(DbLocalService dbLocalService)
    {
        _dbLocalService = dbLocalService;
    }

    public async Task<UserSession?> GetUserSessionById(Guid sessionId)
    {
        var session = await _dbLocalService.GetSessionById(sessionId);

        if (session == null)
        {
            return null;
        }

        var userSession = new UserSession
        {
            Session = session,
            User = await _dbLocalService.GetUserById(session.Email) ?? new User()
        };

        return userSession;
    }

    public async Task<List<UserSession>> GetUserSessions()
    {
        var userSessions = new List<UserSession>();

        var sessions = await _dbLocalService.GetSessions();

        foreach (var session in (await _dbLocalService.GetSessions()).Where(x => string.IsNullOrEmpty(x.Email)).ToList())
        {
            await _dbLocalService.DeleteSession(session);
        }

        sessions = await _dbLocalService.GetSessions();

        foreach (var session in sessions)
        {
            var user = await _dbLocalService.GetUserById(session.Email);

            if (user is null)
            {
                continue;
            }

            userSessions.Add(new UserSession
            {
                Session = session,
                User = user
            });
        }

        return userSessions;
    }

    public async Task SaveUserSession(List<UserSession> userSessions)
    {
        foreach (var session in (await _dbLocalService.GetSessions()).Where(x => string.IsNullOrEmpty(x.Email)).ToList())
        {
            await _dbLocalService.DeleteSession(session);
        }

        foreach (var userSession in userSessions)
        {
            if (await _dbLocalService.GetSessionById(userSession.Session.Id) is null)
            {
                userSession.Session.Email = userSession.User.Email;
                await _dbLocalService.CreateSession(userSession.Session);

                if (await _dbLocalService.GetUserById(userSession.User.Email) is null)
                {
                    await _dbLocalService.CreateUser(userSession.User);
                }
            }
            else
            {
                userSession.Session.Email = userSession.User.Email;
                await _dbLocalService.UpdateSession(userSession.Session);
            }
        }
    }

    public async Task DeleteSession(Session session)
    {
        await _dbLocalService.DeleteSession(session);
    }

    public async Task<List<Message>> GetMessages(Guid sessionId)
    {
        return await _dbLocalService.GetMessagesBySessionId(sessionId);
    }

    public async Task SaveMessage(Message message)
    {
        await _dbLocalService.CreateMessage(message);
    }

    public async Task<Message?> GetLastMessage(Guid sessionId)
    {
        return await _dbLocalService.GetLastMessage(sessionId);
    }

    public async Task<bool> ExistsMessage(Guid messageId)
    {
        return await _dbLocalService.ExistsMessage(messageId);
    }
}

