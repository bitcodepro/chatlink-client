using ChatLink.Client.Services.Interfaces;

namespace ChatLink.Client.Pages;

[QueryProperty("email", "email")]
public partial class VideoOtherPage : ContentPage
{
    private readonly IUserService _userService;

    public string email { get; set; }

    public VideoOtherPage(IUserService userService)
    {
        _userService = userService;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var permissionsGranted = await CheckAndRequestAudioPermissions();
        if (!permissionsGranted)
        {
            await DisplayAlert("Permissions Denied", "Audio permissions are required to record audio.", "OK");
            return;
        }

        Shell.SetTabBarIsVisible(this, false);

        LoadWebViewContent();
    }

    private async void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        string? jwtToken = await _userService.GetJwtToken();
        CustomWebView.Eval($"start('{jwtToken}','{email}','1')");
    }

    private void LoadWebViewContent()
    {
        CustomWebView.Source = new UrlWebViewSource
        {
            Url = "file:///android_asset/hybrid_root/index.html"
        };
    }

    public async Task<bool> CheckAndRequestAudioPermissions()
    {
        var recordAudioStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        var recordCameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        var writeStorageStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

        if (recordAudioStatus != PermissionStatus.Granted)
        {
            recordAudioStatus = await Permissions.RequestAsync<Permissions.Microphone>();
        }

        if (writeStorageStatus != PermissionStatus.Granted)
        {
            writeStorageStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        if (recordCameraStatus != PermissionStatus.Granted)
        {
            recordCameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return recordAudioStatus == PermissionStatus.Granted &&
               writeStorageStatus == PermissionStatus.Granted &&
               recordCameraStatus == PermissionStatus.Granted;
    }
}