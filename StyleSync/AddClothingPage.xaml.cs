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
    public partial class AddClothingPage : ContentPage
    {
        private string selectedImagePath;
        private string selectedColorHex;

        int userId = SessionManager.CurrentUserId;


        public AddClothingPage()
        {
            InitializeComponent();
            InitializeColorCollection();
        }

        private void InitializeColorCollection()
        {
            ColorCollection.ItemsSource = new List<Color>
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
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Color selected)
            {
                selectedColorHex = selected.ToHex(); // Save HEX color
            }
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select a clothing image"
                });

                if (result != null)
                {
                    var fileName = Path.GetFileName(result.FullPath);
                    var newPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                    using (var sourceStream = await result.OpenReadAsync())
                    using (var destinationStream = File.Create(newPath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    selectedImagePath = newPath;
                    ClothingImagePreview.Source = ImageSource.FromFile(newPath);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || TypePicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Name and Type are required.", "OK");
                return;
            }

            var newItem = new ClothingItem
            {
                UserId = SessionManager.CurrentUserId, // ✅ Link to current user
                Name = NameEntry.Text,
                Type = TypePicker.SelectedItem.ToString(),
                Color = selectedColorHex,
                Size = SizePicker.SelectedItem?.ToString(),
                SuitableOccasion = OccasionPicker.SelectedItem?.ToString(),
                WeatherSuitability = WeatherPicker.SelectedItem?.ToString(),
                ImagePath = selectedImagePath ?? "",
                IsClean = IsCleanSwitch.IsToggled,
                IsAvailable = IsAvailableSwitch.IsToggled,
                LastWornDate = null,
                TimesWorn = 0
            };

            await DatabaseService.SaveItemAsync(newItem);
            await DisplayAlert("Success", "Clothing item saved successfully!", "OK");

            // Reset form
            NameEntry.Text = string.Empty;
            TypePicker.SelectedIndex = -1;
            selectedColorHex = null;
            ColorCollection.SelectedItem = null;
            SizePicker.SelectedIndex = -1;
            OccasionPicker.SelectedIndex = -1;
            WeatherPicker.SelectedIndex = -1;
            IsCleanSwitch.IsToggled = false;
            IsAvailableSwitch.IsToggled = false;
            ClothingImagePreview.Source = null;
            selectedImagePath = null;
        }

    }
}
