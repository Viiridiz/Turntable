namespace Turntable;

public partial class SellerHomePage : ContentPage
{
    public SellerHomePage(string email)
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
            window.Page = new NavigationPage(new MainPage());
        }
    }
}