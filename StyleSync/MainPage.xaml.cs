using Microsoft.Maui.Controls;
using StyleSync.Services;
namespace StyleSync
{
    public partial class MainPage : ContentPage
    {

        int userId = SessionManager.CurrentUserId;


        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnWeatherForecastClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WeatherPage());
        }

        private async void OnOutfitSuggestionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OutfitPage());
        }

        private async void OnStyleOutfitClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StyleOutfitPage());
        }


        private async void OnWardrobeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WardrobePage());
        }

        private async void OnOutfitHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RestoreOutfitsPage());
        }

        private async void OnResetDatabaseClicked(object sender, EventArgs e)
        {
            await DatabaseService.ResetDatabaseAsync();
            await DisplayAlert("Reset", "Database has been reset!", "OK");
        }


        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InventoryPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            SessionManager.CurrentUserId = 0; // clear the session
            await Navigation.PushAsync(new LoginPage()); // navigate to login page
        }

    }
}
