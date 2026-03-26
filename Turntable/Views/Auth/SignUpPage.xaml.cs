using Newtonsoft.Json.Linq;
using Turntable.Services;
using Turntable.Models;
using Turntable.Views.Seller;
using Turntable.Views.Buyer;

namespace Turntable.Views.Auth;

public partial class SignUpPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();
    private FirestoreService _firestoreService = new FirestoreService();
    private string _selectedRole = "Buyer";

    public SignUpPage()
    {
        InitializeComponent();
    }

    private void OnBuyerClicked(object sender, EventArgs e)
    {
        _selectedRole = "Buyer";
        BuyerBtn.BackgroundColor = Color.FromArgb("#d4af35");
        BuyerBtn.TextColor = Color.FromArgb("#1a1812");
        SellerBtn.BackgroundColor = Colors.Transparent;
        SellerBtn.TextColor = Color.FromArgb("#94a3b8");
    }

    private void OnSellerClicked(object sender, EventArgs e)
    {
        _selectedRole = "Seller";
        SellerBtn.BackgroundColor = Color.FromArgb("#d4af35");
        SellerBtn.TextColor = Color.FromArgb("#1a1812");
        BuyerBtn.BackgroundColor = Colors.Transparent;
        BuyerBtn.TextColor = Color.FromArgb("#94a3b8");
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            var rawResponse = await _authService.SignUp(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty
            );

            var authData = JObject.Parse(rawResponse);
            string idToken = authData["idToken"]?.ToString() ?? string.Empty;

            var newUser = new UserModel
            {
                Uid = authData["localId"]?.ToString() ?? string.Empty,
                Email = EmailEntry.Text ?? string.Empty,
                Role = _selectedRole,
                FullName = FullNameEntry.Text ?? string.Empty
            };

            await _firestoreService.CreateUserDocument(newUser, idToken);

            await SecureStorage.SetAsync("firebase_token", idToken);
            Preferences.Set("user_role", newUser.Role);
            Preferences.Set("user_uid", newUser.Uid);

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                if (newUser.Role == "Seller")
                {
                    window.Page = new NavigationPage(new SellerHomePage(newUser.Email));
                }
                else
                {
                    window.Page = new NavigationPage(new CollectorHomePage(newUser.Email));
                }
            }
        }
        catch (Exception ex)
        {
            result.Text = "Sign up failed. Check your details.";
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnGoToSignInPage(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}