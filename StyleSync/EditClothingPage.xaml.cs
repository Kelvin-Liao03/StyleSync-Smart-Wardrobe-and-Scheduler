using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using StyleSync.Models;
using StyleSync.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StyleSync
{
    public partial class EditClothingPage : ContentPage
    {
        private ClothingItem clothingItem;
        private string selectedColorHex;

        int userId = SessionManager.CurrentUserId;

        public EditClothingPage(ClothingItem item)
        {
            InitializeComponent();
            clothingItem = item;
            InitializeColorCollection();
            LoadItemData();
        }

        private void InitializeColorCollection()
        {
            var colorList = new List<Color>
            {
                Colors.White, Colors.Black, Colors.Gray, Colors.Red, Colors.Blue,
                Colors.LightBlue, Colors.Navy, Colors.Green, Colors.Olive, Colors.Yellow,
                Colors.Beige, Colors.Brown, Colors.Pink, Colors.Purple, Colors.Maroon,
                Colors.Orange, Colors.Gold, Colors.Silver, Colors.Teal, Colors.Turquoise,
                Colors.Lavender,
                Color.FromArgb("#FFFDD0"), // Cream
                Colors.Khaki,
                Color.FromArgb("#36454F")  // Charcoal
            };

            ColorCollection.ItemsSource = colorList;

            if (!string.IsNullOrWhiteSpace(clothingItem.Color))
            {
                var matchedColor = colorList.FirstOrDefault(c => c.ToHex().Equals(clothingItem.Color, StringComparison.OrdinalIgnoreCase));
                if (matchedColor != null)
                {
                    ColorCollection.SelectedItem = matchedColor;
                    selectedColorHex = matchedColor.ToHex();
                }
            }
        }

        private void LoadItemData()
        {
            NameEntry.Text = clothingItem.Name;
            TypePicker.SelectedItem = clothingItem.Type;
            ClothingImagePreview.Source = !string.IsNullOrWhiteSpace(clothingItem.ImagePath)
                ? ImageSource.FromFile(clothingItem.ImagePath)
                : null;

            SizePicker.SelectedItem = clothingItem.Size;
            OccasionPicker.SelectedItem = clothingItem.SuitableOccasion;
            WeatherPicker.SelectedItem = clothingItem.WeatherSuitability;

            IsCleanSwitch.IsToggled = clothingItem.IsClean;
            IsAvailableSwitch.IsToggled = clothingItem.IsAvailable;
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Color selected)
            {
                selectedColorHex = selected.ToHex();
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            clothingItem.Name = NameEntry.Text;
            clothingItem.Type = TypePicker.SelectedItem?.ToString();
            clothingItem.Color = selectedColorHex;
            clothingItem.Size = SizePicker.SelectedItem?.ToString();
            clothingItem.SuitableOccasion = OccasionPicker.SelectedItem?.ToString();
            clothingItem.WeatherSuitability = WeatherPicker.SelectedItem?.ToString();
            clothingItem.IsClean = IsCleanSwitch.IsToggled;
            clothingItem.IsAvailable = IsAvailableSwitch.IsToggled;
            clothingItem.UserId = userId; // ✅ Ensure item is linked to correct user

            await DatabaseService.SaveItemAsync(clothingItem);
            await DisplayAlert("Updated", "Clothing item updated successfully!", "OK");
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");
            if (confirm)
            {
                await DatabaseService.DeleteItemAsync(clothingItem);
                await DisplayAlert("Deleted", "Clothing item deleted successfully!", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
