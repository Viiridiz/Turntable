namespace Turntable.Views.Auth;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(3000);

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            window.Page = new NavigationPage(new SignInPage());
        }
    }
}