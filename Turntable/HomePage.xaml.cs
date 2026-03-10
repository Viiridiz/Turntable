namespace Turntable;

public partial class HomePage : ContentPage
{
    public HomePage(string email)
    {
        InitializeComponent();
        EmailLabel.Text = $"Logged in as {email}";
    }

    private void OnLogOutClicked(object sender, EventArgs e)
    {
        SecureStorage.Remove("firebase_token");
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}