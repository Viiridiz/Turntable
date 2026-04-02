using Turntable.Views.Auth;

namespace Turntable.Views.Seller;

public partial class SellerHomePage : ContentPage
{
    public SellerHomePage(string email)
    {
        InitializeComponent();
        EmailLabel.Text = email;
    }

    private async void OnPostListingClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddListingPage());
    }

    private async void OnGoToInventoryTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyInventoryPage());
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