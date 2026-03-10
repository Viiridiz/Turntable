using Newtonsoft.Json.Linq;
using Turntable.Services;
using Turntable.Models;

namespace Turntable;

public partial class SignUpPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();
    private FirestoreService _firestoreService = new FirestoreService();

    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            string? selectedRole = RolePicker.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedRole))
            {
                result.Text = "Please select a role.";
                return;
            }

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
                Role = selectedRole
            };

            await _firestoreService.CreateUserDocument(newUser, idToken);

            await SecureStorage.SetAsync("firebase_token", idToken);
            Preferences.Set("user_role", newUser.Role);

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
        await Navigation.PushAsync(new SignInPage());
    }
}