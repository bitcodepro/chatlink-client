using ChatLink.Client.ViewModels;

namespace ChatLink.Client.Pages;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(RegistrationViewModel registrationViewModel)
	{
		InitializeComponent();
        BindingContext = registrationViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetTabBarIsVisible(this, false);
    }
}