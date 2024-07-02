using ChatLink.Client.Pages;
using ChatLink.Client.Providers;

namespace ChatLink.Client;

public partial class AppShell : Shell
{
    public AppShell(ChatsDataProvider chatsDataProvider)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        Routing.RegisterRoute(nameof(ChatsPage), typeof(ChatsPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
        Routing.RegisterRoute(nameof(VideoPage), typeof(VideoPage));
        Routing.RegisterRoute(nameof(VideoOtherPage), typeof(VideoOtherPage));

        chatsDataProvider.UpdateGetUserSession();
    }
}
