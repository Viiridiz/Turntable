using Turntable.Services;

namespace Turntable.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();

    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ResultMessage.TextColor = Colors.Red;
            ResultMessage.Text = "Please enter an email address.";
            return;
        }

        try
        {
            await _authService.ResetPassword(EmailEntry.Text.Trim());
            ResultMessage.TextColor = Colors.Green;
            ResultMessage.Text = "Reset link sent! Check your inbox.";
        }
        catch (Exception)
        {
            ResultMessage.TextColor = Colors.Red;
            ResultMessage.Text = "Failed to send reset link. Check the email and try again.";
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}