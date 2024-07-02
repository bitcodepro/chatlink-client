using ChatLink.Client.Models;
using SQLite;

namespace ChatLink.Client.Services;

public class DbLocalService
{
    public const SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLiteOpenFlags.SharedCache;

    private const string DbName = "chatlink.db3";
    private SQLiteAsyncConnection? _connection;

    private async Task Init()
    {
        if (_connection is not null)
            return;

        _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DbName));

        await _connection.CreateTableAsync<User>();
        await _connection.CreateTableAsync<Session>();
        await _connection.CreateTableAsync<Message>();
    }

    #region User

    public async Task<List<User>> GetUsers(List<string>? ids = null)
    {
        await Init();
        return ids == null ? await _connection.Table<User>().ToListAsync() : await _connection.Table<User>().Where(x => ids.Contains(x.Email)).ToListAsync();
    }

    public async Task<User?> GetUserById(string email)
    {
        await Init();
        return await _connection.Table<User>().Where(x => x.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateUser(User user)
    {
        await Init();
        await _connection.InsertAsync(user);
    }

    public async Task UpdateUser(User user)
    {
        await Init();
        await _connection.UpdateAsync(user);
    }

    public async Task DeleteUser(User user)
    {
        await Init();
        await _connection.DeleteAsync(user);
    }

    public async Task DeleteUsers()
    {
        await Init();
        await _connection.DeleteAllAsync<User>();
    }

    #endregion User

    #region Session

    public async Task<List<Session>> GetSessions()
    {
        await Init();
        return await _connection.Table<Session>().ToListAsync();
    }

    public async Task<Session?> GetSessionById(Guid id)
    {
        await Init();
        return await _connection.Table<Session>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateSession(Session session)
    {
        await Init();
        await _connection.InsertAsync(session);
    }

    public async Task UpdateSession(Session session)
    {
        await Init();
        await _connection.UpdateAsync(session);
    }

    public async Task DeleteSession(Session session)
    {
        await Init();
        await _connection.DeleteAsync(session);
    }

    public async Task DeleteSessions()
    {
        await Init();
        await _connection.DeleteAllAsync<Session>();
    }

    #endregion Session

    #region Message

    public async Task<List<Message>> GetMessagesBySessionId(Guid sessionId)
    {
        await Init();
        return await _connection.Table<Message>().Where(x => x.SessionId == sessionId).OrderBy(x => x.CreatedDt).ToListAsync();
    }

    public async Task<List<Message>> GetAllMessages()
    {
        await Init();
        return await _connection.Table<Message>().ToListAsync();
    }

    public async Task<Message?> GetLastMessage(Guid sessionId)
    {
        await Init();
        return await _connection.Table<Message>().Where(x => x.SessionId == sessionId).OrderByDescending(x => x.CreatedDt).FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessageById(Guid id)
    {
        await Init();
        return await _connection.Table<Message>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateMessage(Message message)
    {
        await Init();
        await _connection.InsertAsync(message);
    }

    public async Task<bool> ExistsMessage(Guid messageId)
    {
        await Init(); 
        return (await _connection.Table<Message>().FirstOrDefaultAsync(x => x.Id == messageId)) != null;
    }

    public async Task UpdateMessage(Message message)
    {
        await Init();
        await _connection.UpdateAsync(message);
    }

    public async Task DeleteMessage(Message message)
    {
        await Init();
        await _connection.DeleteAsync(message);
    }

    public async Task DeleteMessages()
    {
        await Init();
        await _connection.DeleteAllAsync<Message>();
    }

    #endregion Message
}
