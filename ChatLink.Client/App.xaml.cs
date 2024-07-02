using System.Net;
using System.Timers;
using ChatLink.Client.CustomControls;
using ChatLink.Client.Pages;
using ChatLink.Client.Providers;
using ChatLink.Client.Services.Interfaces;
using Timer = System.Timers.Timer;

namespace ChatLink.Client
{
    public partial class App : Application
    {
        private bool previusState;
        private Timer connectivityCheckTimer;
        private readonly IUserService _userService;

        public App(AppShell appShell, IUserService userService, ChatDataProvider chatDataProvider)
        {
            _userService = userService;
            InitializeComponent();

            MainPage = appShell;

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
            {
#if __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.White;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif __ANDROID__
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) => {
#if __ANDROID__
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            //SetupConnectivityPolling();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                await _userService.StartSignalR();
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
            {
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
            catch (Exception e)
            {

            }
        }

        protected override async void OnSleep()
        {
            base.OnSleep();

            previusState = ChatDataProvider.CanShowNotification;
            ChatDataProvider.CanShowNotification = true;

            try
            {
                //await _userService.StopSignalR();
            }
            catch (Exception e)
            {

            }
        }

        protected override async void OnResume()
        {
            base.OnResume();

            ChatDataProvider.CanShowNotification = previusState;

            try
            {
                await _userService.StartSignalR();
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
            {
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
            catch (Exception e)
            {

            }
        }

        protected override async void CleanUp()
        {
            base.CleanUp();

            try
            {
                await _userService.StopSignalR();
            }
            catch (Exception e)
            {

            }
        }

        private void SetupConnectivityPolling()
        {
            connectivityCheckTimer = new Timer(20000); // Check every 10 seconds
            connectivityCheckTimer.Elapsed += CheckConnectivityStatus;
            connectivityCheckTimer.AutoReset = true;
            connectivityCheckTimer.Enabled = true;
        }

        private void CheckConnectivityStatus(object sender, ElapsedEventArgs e)
        {
            var currentAccess = Connectivity.Current.NetworkAccess;

            if (currentAccess == NetworkAccess.Internet)
            {
                // Internet is available
                Console.WriteLine("Internet is available.");
            }
            else if (currentAccess == NetworkAccess.None)
            {
                // Internet is not available
                Console.WriteLine("Internet is unavailable.");
            }
        }
    }
}
