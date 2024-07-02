namespace ChatLink.Client.Pages;

public partial class FullScreenImagePage : ContentPage
{
    public FullScreenImagePage(string imagePath)
    {
        InitializeComponent();
        FullScreenImage.Source = ImageSource.FromFile(imagePath);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, false);
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSwiped(object sender, SwipedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width > height)
        {
            VisualStateManager.GoToState(this, "Landscape");
        }
        else
        {
            VisualStateManager.GoToState(this, "Portrait");
        }
    }
}