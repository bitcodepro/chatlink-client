using ChatLink.Client.Models;
using ChatLink.Client.Models.Dtos;

namespace ChatLink.Client.Services.Interfaces;

public interface ISessionService
{
    public Task<UserSession?> GetUserSessionById(Guid sessionId);
    public Task<List<UserSession>> GetUserSessions();
    public Task SaveUserSession(List<UserSession> userSessions);
    public Task DeleteSession(Session session);

    public Task<List<Message>> GetMessages(Guid sessionId);
    public Task SaveMessage(Message message);

    public Task<Message?> GetLastMessage(Guid sessionId);

    public Task<bool> ExistsMessage(Guid messageId);
}
