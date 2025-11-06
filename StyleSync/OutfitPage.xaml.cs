// OutfitPage.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using StyleSync.Models;
using StyleSync.Services;

namespace StyleSync
{
    public partial class OutfitPage : ContentPage
    {
        private ClothingItem selectedTop;
        private ClothingItem selectedBottom;
        private ClothingItem selectedDress;

        private List<ClothingItem> wardrobeItems;
        private List<ClothingItem> upperItems;
        private List<ClothingItem> lowerItems;
        private ClothingItem dressItem;
        private string tempCategory;
        private string currentOccasion;
        private Random random = new();

        int userId = SessionManager.CurrentUserId;

        public OutfitPage()
        {
            InitializeComponent();
        }

        private async Task PrepareOutfitDataAsync(string occasion)
        {
            double temperature = await FetchCurrentTemperatureAsync();

            tempCategory = temperature >= 27 ? "Hot" :
                           temperature >= 20 ? "Warm" :
                           temperature >= 15 ? "Cool" : "Cold";

            wardrobeItems = await DatabaseService.GetItemsByUserIdAsync(userId);

            var filteredItems = wardrobeItems
                .Where(item =>
                    !string.IsNullOrEmpty(item.SuitableOccasion) &&
                    item.SuitableOccasion.Contains(occasion, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(item.WeatherSuitability) &&
                    item.WeatherSuitability.Contains(tempCategory, StringComparison.OrdinalIgnoreCase) &&
                    item.IsClean && item.IsAvailable)
                .ToList();

            upperItems = filteredItems.Where(i => new[] { "T-shirt", "Shirt", "Blouse", "Hoodie", "Sweater", "Jacket", "Coat", "Tank Top", "Cardigan" }.Contains(i.Type)).ToList();
            lowerItems = filteredItems.Where(i => new[] { "Pants", "Jeans", "Shorts", "Skirt" }.Contains(i.Type)).ToList();
            dressItem = filteredItems.FirstOrDefault(i => i.Type == "Dress");
        }

        private string GenerateOutfitSuggestion()
        {
            selectedTop = null;
            selectedBottom = null;
            selectedDress = null;

            if (dressItem != null && upperItems.Count == 0 && lowerItems.Count == 0)
            {
                selectedDress = dressItem;
                TopImage.Source = ImageSource.FromFile(dressItem.ImagePath);
                TopLabel.Text = $"\ud83d\udc57 Outfit: {dressItem.Name}\n\ud83c\udfa8 Color: {GetColorName(dressItem.Color)}";
                BottomImage.Source = null;
                BottomLabel.Text = "";
                return dressItem.Name;
            }

            if (upperItems.Count == 0 || lowerItems.Count == 0)
                return "\u274c No suitable outfit found based on weather and occasion.";

            var top = upperItems[random.Next(upperItems.Count)];
            var matchingBottoms = lowerItems
                .Where(b => AreColorsMatching(top.Color?.ToLower() ?? "", b.Color?.ToLower() ?? ""))
                .ToList();

            var bottom = matchingBottoms.Any()
                ? matchingBottoms[random.Next(matchingBottoms.Count)]
                : lowerItems[random.Next(lowerItems.Count)];

            selectedTop = top;
            selectedBottom = bottom;

            TopImage.Source = ImageSource.FromFile(top.ImagePath);
            BottomImage.Source = ImageSource.FromFile(bottom.ImagePath);
            TopLabel.Text = $"\ud83d\udc55 Top: {top.Name}\n\ud83c\udfa8 Color: {GetColorName(top.Color)}";
            BottomLabel.Text = $"\ud83d\udc56 Bottom: {bottom.Name}\n\ud83c\udfa8 Color: {GetColorName(bottom.Color)}";

            return $"{top.Name} + {bottom.Name}";
        }

        private async void OnSuggestClicked(object sender, EventArgs e)
        {
            TopImage.Source = BottomImage.Source = null;
            TopLabel.Text = BottomLabel.Text = suggestionLabel.Text = "";
            OutfitContainer.IsVisible = false;
            PickButton.IsVisible = SkipButton.IsVisible = eventDatePicker.IsVisible = false;

            string occasion = occasionPicker.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(occasion))
            {
                suggestionLabel.Text = "\u26a0 Please select an occasion first.";
                return;
            }

            currentOccasion = occasion;
            await PrepareOutfitDataAsync(currentOccasion);
            string suggestion = GenerateOutfitSuggestion();

            suggestionLabel.Text = $"\u2705 Suggested outfit for {occasion}:\n{suggestion}";
            if (!suggestion.StartsWith("\u274c"))
            {
                OutfitContainer.IsVisible = true;
                PickButton.IsVisible = SkipButton.IsVisible = eventDatePicker.IsVisible = true;
            }
        }

        private void OnSkipClicked(object sender, EventArgs e)
        {
            string suggestion = GenerateOutfitSuggestion();
            suggestionLabel.Text = $"\u2705 Suggested outfit for {currentOccasion}:\n{suggestion}";
        }

        private async void OnPickOutfitClicked(object sender, EventArgs e)
        {
            string occasion = currentOccasion ?? "Unspecified";
            DateTime selectedDate = eventDatePicker.Date;

            if (selectedDate == default)
            {
                await DisplayAlert("Date Required", "Please select a valid event date.", "OK");
                return;
            }

            if (selectedDress != null)
            {
                selectedDress.IsAvailable = false;
                selectedDress.IsClean = false;
                await DatabaseService.SaveItemAsync(selectedDress);

                await DatabaseService.SaveOutfitHistoryAsync(new OutfitHistory
                {
                    EventDate = selectedDate,
                    Occasion = occasion,
                    TopItemName = selectedDress.Name,
                    BottomItemName = null,
                    IsReturned = false,
                    UserId = userId
                });
            }
            else if (selectedTop != null && selectedBottom != null)
            {
                selectedTop.IsAvailable = false;
                selectedTop.IsClean = false;
                selectedBottom.IsAvailable = false;
                selectedBottom.IsClean = false;

                await DatabaseService.SaveItemAsync(selectedTop);
                await DatabaseService.SaveItemAsync(selectedBottom);

                await DatabaseService.SaveOutfitHistoryAsync(new OutfitHistory
                {
                    EventDate = selectedDate,
                    Occasion = occasion,
                    TopItemName = selectedTop.Name,
                    BottomItemName = selectedBottom.Name,
                    IsReturned = false,
                    UserId = userId
                });
            }

            await DisplayAlert("Saved", "Outfit saved and marked as worn. Inventory updated.", "OK");

            OutfitContainer.IsVisible = false;
            PickButton.IsVisible = SkipButton.IsVisible = eventDatePicker.IsVisible = false;
            suggestionLabel.Text = "";
            occasionPicker.SelectedIndex = 0;
        }

        private string GetColorName(string hex)
        {
            var colorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "#000000", "Black" },
                { "#FFFFFF", "White" },
                { "#FF0000", "Red" },
                { "#0000FF", "Blue" },
                { "#00FF00", "Green" },
                { "#FFFF00", "Yellow" },
                { "#FFA500", "Orange" },
                { "#800080", "Purple" },
                { "#808080", "Gray" },
                { "#A52A2A", "Brown" }
            };

            if (colorMap.TryGetValue(hex, out var name))
                return name;

            return "Unknown";
        }

        private bool AreColorsMatching(string color1, string color2)
        {
            var matchGroups = new List<List<string>>
            {
                new() { "white", "black", "gray", "silver", "cream", "beige", "charcoal" },
                new() { "red", "maroon", "pink" },
                new() { "blue", "navy", "lightblue", "turquoise", "lavender" },
                new() { "green", "olive", "teal" },
                new() { "yellow", "gold", "orange" },
                new() { "brown", "khaki" },
                new() { "purple" }
            };

            foreach (var group in matchGroups)
            {
                if (group.Contains(color1) && group.Contains(color2))
                    return true;
            }

            return false;
        }

        private async Task<double> FetchCurrentTemperatureAsync()
        {
            string apiKey = "cb920f35c6d565b38461ab291e2e064b";
            string city = "Singapore";
            string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                using HttpClient client = new();
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var weatherData = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream);
                    return weatherData.GetProperty("main").GetProperty("temp").GetDouble();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Weather Error", ex.Message, "OK");
            }

            return 25.0;
        }
    }
}