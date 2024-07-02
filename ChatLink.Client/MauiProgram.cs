using ChatLink.Client.CustomControls;
using ChatLink.Client.Models;
using ChatLink.Client.Pages;
using ChatLink.Client.Platforms.Android;
using ChatLink.Client.Providers;
using ChatLink.Client.Providers.Handlers;
using ChatLink.Client.Services;
using ChatLink.Client.Services.Interfaces;
using ChatLink.Client.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;
using The49.Maui.BottomSheet;

namespace ChatLink.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .UseBottomSheet()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddFont("Font-Awesome-Brands-Regular-400.otf", "FontAwesomeBrandsRegular");
                    fonts.AddFont("Font-Awesome-Free-Regular-400.otf", "FontAwesomeFreeRegular");
                    fonts.AddFont("Font-Awesome-Free-Solid-900.otf", "FontAwesomeFreeSolid");
                });


#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(AudioManager.Current);

            builder.UseLocalNotification();

            builder.Services.AddSingleton<DbLocalService>();
            builder.Services.AddTransient<FileService>();

            builder.Services.AddSingleton<ContactHandler>();
            builder.Services.AddSingleton<ChatsHandler>();
            builder.Services.AddSingleton<ChatHandler>();

            builder.Services.AddSingleton<ContactDataProvider>();
            builder.Services.AddSingleton<ChatsDataProvider>();
            builder.Services.AddSingleton<ChatDataProvider>();

            builder.Services.AddSingleton<EncryptionManager>();
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

            builder.Services.AddSingleton<HttpClient>(x =>
            {
                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                return client;
            });

            builder.Services.AddSingleton<IApiService, ApiService>();

            builder.Services.AddSingleton<ISignalRService, SignalRService>();

            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();

            builder.Services.AddSingleton<ChatsViewModel>();
            builder.Services.AddSingleton<ChatsPage>();

            builder.Services.AddSingleton<ContactsViewModel>();
            builder.Services.AddSingleton<ContactsPage>();

            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddSingleton<RegistrationViewModel>();
            builder.Services.AddSingleton<RegistrationPage>();

            builder.Services.AddSingleton<SettingsPage>();

            builder.Services.AddTransient<VideoPage>();
            builder.Services.AddTransient<VideoOtherPage>();

            builder.Services.AddSingleton<AppShell>();

            var app = builder.Build();

            CustomizeWebViewHandler();

            return app;
        }

        private static void CustomizeWebViewHandler()
        {
#if ANDROID26_0_OR_GREATER
    Microsoft.Maui.Handlers.WebViewHandler.Mapper.ModifyMapping(
        nameof(Android.Webkit.WebView.WebChromeClient),
        (handler, view, args) => handler.PlatformView.SetWebChromeClient(new MyWebChromeClient(handler)));
#endif
        }
    }

}
