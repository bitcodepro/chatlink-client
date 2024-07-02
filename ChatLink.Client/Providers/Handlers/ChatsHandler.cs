namespace ChatLink.Client.Providers.Handlers;

public class ChatsHandler
{
    public event Action OnUpdateChats;
    public event Action OnUpdateLastMessageInChat;

    public bool needUpdateChats = false;

    public void UpdateChats()
    {
        needUpdateChats = true;
        OnUpdateChats?.Invoke();
    }

    public void UpdateLastMessageInChat()
    {
        OnUpdateLastMessageInChat?.Invoke();
    }
}