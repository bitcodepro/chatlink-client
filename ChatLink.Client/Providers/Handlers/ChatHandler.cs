namespace ChatLink.Client.Providers.Handlers;

public class ChatHandler
{
    public event Action<string, string> OnUpdateChat;
    public event Action<string> OnReceiveTheCall;
    public event Action OnFinishCall;
    public bool needUpdateChat = false;

    public void UpdateChat(string email, string encryptedMessage)
    {
        needUpdateChat = true;
        OnUpdateChat?.Invoke(email, encryptedMessage);
    }    
    
    public void ReceiveTheCall(string email)
    {
        OnReceiveTheCall?.Invoke(email);
    }

    public void FinishCall()
    {
        OnFinishCall?.Invoke();
    }
}