using Turntable.Views.Auth;

namespace Turntable.Views.Buyer;

public partial class CollectorHomePage : ContentPage
{
    public CollectorHomePage(string email)
    {
        InitializeComponent();
        EmailLabel.Text = email;
    }

    private void OnLogOutClicked(object sender, EventArgs e)
    {
        SecureStorage.Remove("firebase_token");
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            window.Page = new NavigationPage(new SignInPage());
        }
    }
}