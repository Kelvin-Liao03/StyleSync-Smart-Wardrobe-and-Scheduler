using StyleSync.Models;
using StyleSync.Services;

namespace StyleSync;

public partial class RegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public RegisterPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;
        string gender = GenderPicker.SelectedItem?.ToString() ?? "";
        string shoppingFreq = ShoppingPicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        bool usernameTaken = await _databaseService.IsUsernameTakenAsync(username);
        if (usernameTaken)
        {
            await DisplayAlert("Error", "Username is already taken.", "OK");
            return;
        }

        string events = "";
        if (WeddingSwitch.IsToggled) events += "Weddings, ";
        if (MeetingSwitch.IsToggled) events += "Meetings, ";
        if (SportsSwitch.IsToggled) events += "Sports/Gym, ";
        if (CasualSwitch.IsToggled) events += "Casual Hangouts, ";
        events = events.TrimEnd(',', ' ');

        string clothingStyles = "";
        if (CasualStyleSwitch.IsToggled) clothingStyles += "Casual, ";
        if (FormalStyleSwitch.IsToggled) clothingStyles += "Formal, ";
        if (SportyStyleSwitch.IsToggled) clothingStyles += "Sporty, ";
        if (TraditionalStyleSwitch.IsToggled) clothingStyles += "Traditional, ";
        clothingStyles = clothingStyles.TrimEnd(',', ' ');

        var newUser = new User
        {
            Username = username,
            Email = email,
            Password = password,
            Gender = gender,
            UsualEvents = events,
            ClothingStyles = clothingStyles,
            ShoppingFrequency = shoppingFreq
        };

        await _databaseService.AddUserAsync(newUser);
        await DisplayAlert("Success", "Account created successfully!", "OK");
        await Navigation.PopAsync(); // Return to LoginPage
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
