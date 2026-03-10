namespace Turntable;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}