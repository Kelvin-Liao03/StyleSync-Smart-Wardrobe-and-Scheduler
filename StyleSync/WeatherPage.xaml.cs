using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace StyleSync
{
    public partial class WeatherPage : ContentPage
    {
        private const string ApiKey = "cb920f35c6d565b38461ab291e2e064b"; // ✅ Removed \r\n
        private const string City = "Singapore";

        public WeatherPage()
        {
            InitializeComponent();
            GetWeatherAsync();
        }

        private async void GetWeatherAsync()
        {
            try
            {
                using HttpClient client = new();
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={"Singapore"}&appid={"cb920f35c6d565b38461ab291e2e064b"}&units=metric";

                var response = await client.GetStringAsync(url);
                var data = JObject.Parse(response);

                string description = data["weather"]?[0]?["description"]?.ToString() ?? "unknown";
                string temperature = data["main"]?["temp"]?.ToString() ?? "N/A";

                string forecast = $"The weather in {City} is {description} with a temperature of {temperature}°C.";
                WeatherResult.Text = forecast;
            }
            catch (Exception ex)
            {
                WeatherResult.Text = "Failed to retrieve weather data.";
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnSpeakWeatherClicked(object sender, EventArgs e)
        {
            string forecastText = WeatherResult.Text;
            if (!string.IsNullOrWhiteSpace(forecastText))
            {
                await TextToSpeech.Default.SpeakAsync(forecastText);
            }
        }
    }
}
