namespace ChatLink.Client.Providers.Handlers;

public class ContactHandler
{
    public event Action OnUpdateContacts;

    public void UpdateContacts()
    {
        OnUpdateContacts?.Invoke();
    }
}