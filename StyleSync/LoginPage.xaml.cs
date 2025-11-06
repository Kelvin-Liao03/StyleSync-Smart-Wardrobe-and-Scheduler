using StyleSync.Models;
using StyleSync.Services;

namespace StyleSync;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private string _generatedCode;
    private string _recoveryEmail;

    public LoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    // ✅ Handle Login
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both username and password.", "OK");
            return;
        }

        var user = await _databaseService.GetUserAsync(username, password);

        if (user != null)
        {
            await DisplayAlert("Success", $"Welcome, {user.Username}!", "OK");
            Application.Current.MainPage = new NavigationPage(new MainPage());
            SessionManager.CurrentUserId = user.Id;
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid username or password.", "OK");
        }
    }

    // ✅ Handle Forgot Password
    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        _recoveryEmail = await DisplayPromptAsync("Password Recovery", "Enter your registered email:");

        if (string.IsNullOrWhiteSpace(_recoveryEmail))
        {
            await DisplayAlert("Error", "Email cannot be empty.", "OK");
            return;
        }

        var user = await _databaseService.GetUserByEmailAsync(_recoveryEmail);

        if (user == null)
        {
            await DisplayAlert("Error", "No user found with that email.", "OK");
            return;
        }

        _generatedCode = new Random().Next(100000, 999999).ToString();

        var emailService = new EmailService();
        bool sent = await emailService.SendVerificationCode(_recoveryEmail, user.Username, _generatedCode);

        if (sent)
        {
            await DisplayAlert("Email Sent", "A verification code has been sent to your email.", "OK");
            await Task.Delay(300); // Slight delay to ensure dialog order
            await PromptForVerificationCode(user);
        }
        else
        {
            await DisplayAlert("Failed", "Failed to send email. Try again later.", "OK");
        }
    }

    // ✅ Prompt user to verify code and reset password
    private async Task PromptForVerificationCode(User user)
    {
        string inputCode = await DisplayPromptAsync("Verify Code", "Enter the 6-digit code sent to your email:");

        if (inputCode == _generatedCode)
        {
            string newPassword = await DisplayPromptAsync("Reset Password", "Enter your new password:");

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                await DisplayAlert("Error", "Password cannot be empty.", "OK");
                return;
            }

            user.Password = newPassword;
            await _databaseService.UpdateUserAsync(user);

            await DisplayAlert("Success", "Your password has been reset. Please login again.", "OK");
        }
        else
        {
            await DisplayAlert("Invalid Code", "The code you entered is incorrect.", "OK");
        }
    }

    // ✅ Navigate to Register Page
    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}
