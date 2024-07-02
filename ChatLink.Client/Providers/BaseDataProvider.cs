namespace ChatLink.Client.Providers;

public abstract class BaseDataProvider
{
    protected Action? _onLogin;
    public event Action OnLogin
    {
        add
        {
            ClearAllHandlers(_onLogin);
            _onLogin += value;
        }
        remove
        {
            _onLogin -= value;
        }
    }


    protected Action? _onDataChanged;
    public event Action OnDataChanged
    {
        add
        {
            ClearAllHandlers(_onDataChanged);
            _onDataChanged += value;
        }
        remove
        {
            _onDataChanged -= value;
        }
    }

    private void ClearAllHandlers(Action? action)
    {
        if (action is null)
        {
            return;
        }

        foreach (Delegate d in action.GetInvocationList())
        {
            _onDataChanged -= (Action)d;
        }
    }
}
