using The49.Maui.BottomSheet;

namespace ChatLink.Client.CustomControls;

public partial class CustomBottomSheet : BottomSheet
{
    private Action _uploadPhotoAction;
    private Action _takePhotoAction;
    private Action _uploadVideoAction;
    private Action _takeVideoAction;

    public CustomBottomSheet(Action uploadPhotoAction, Action takeAction, Action uploadVideoAction, Action takeVideoAction)
	{
		InitializeComponent();

        _uploadPhotoAction = uploadPhotoAction;
        _takePhotoAction = takeAction;
        _uploadVideoAction = uploadVideoAction;
        _takeVideoAction = takeVideoAction;
    }

    private async void ImageButton_OnClicked(object? sender, EventArgs e)
    {
        await DismissAsync(true);
        _uploadPhotoAction?.Invoke();
    }    
    
    private async void CameraButton_OnClicked(object? sender, EventArgs e)
    {
        await DismissAsync(true);
        _takePhotoAction?.Invoke();
    }

    private async void VideoButton_OnClicked(object? sender, EventArgs e)
    {
        await DismissAsync(true);
        _uploadVideoAction?.Invoke();
    }

    private async void VideoCameraButton_OnClicked(object? sender, EventArgs e)
    {
        await DismissAsync(true);
        _takeVideoAction?.Invoke();
    }
}