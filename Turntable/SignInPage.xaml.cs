using Newtonsoft.Json.Linq;
using Turntable.Services;

namespace Turntable;

public partial class SignInPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();
    private FirestoreService _firestoreService = new FirestoreService();

    public SignInPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            var rawResponse = await _authService.SignIn(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty
            );

            var authData = JObject.Parse(rawResponse);
            string idToken = authData["idToken"]?.ToString() ?? string.Empty;
            string localId = authData["localId"]?.ToString() ?? string.Empty;

            string role = await _firestoreService.GetUserRole(localId, idToken);

            await SecureStorage.SetAsync("firebase_token", idToken);
            Preferences.Set("user_role", role);
            Preferences.Set("user_uid", localId);

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                if (role == "Seller")
                {
                    window.Page = new NavigationPage(new SellerHomePage(EmailEntry.Text ?? string.Empty));
                }
                else
                {
                    window.Page = new NavigationPage(new CollectorHomePage(EmailEntry.Text ?? string.Empty));
                }
            }
        }
        catch (Exception ex)
        {
            result.Text = "Sign in failed. Check your credentials.";
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnGoToSignUpPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}