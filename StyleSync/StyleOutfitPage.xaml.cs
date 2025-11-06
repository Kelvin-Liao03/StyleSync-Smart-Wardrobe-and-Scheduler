using System;
using Microsoft.Maui.Controls;
using StyleSync.Models;
using StyleSync.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace StyleSync
{
    public partial class StyleOutfitPage : ContentPage
    {
        private ClothingItem selectedTop;
        private ClothingItem selectedBottom;
        private List<string> unavailableItemNames = new();
        int userId = SessionManager.CurrentUserId;

        public StyleOutfitPage()
        {
            InitializeComponent();
            LoadUnavailableItems();
        }

        private async void LoadUnavailableItems()
        {
            var history = await DatabaseService.GetOutfitHistoryByUserAsync(userId);
            unavailableItemNames = history
                .Where(h => !h.IsReturned)
                .SelectMany(h => new[] { h.TopItemName, h.BottomItemName })
                .Distinct()
                .ToList();

            await LoadTopItems();
            await LoadBottomItems();

            PreviewContainer.IsVisible = false;
            EventDatePicker.IsVisible = false;
            OccasionSection.IsVisible = false;
        }

        private async Task LoadTopItems()
        {
            var tops = await DatabaseService.GetTopItemsByUserAsync(userId);
            foreach (var item in tops)
            {
                item.IsClean = !unavailableItemNames.Contains(item.Name);
            }
            TopCollection.ItemsSource = tops;
        }

        private async Task LoadBottomItems()
        {
            var bottoms = await DatabaseService.GetBottomItemsByUserAsync(userId);
            foreach (var item in bottoms)
            {
                item.IsClean = !unavailableItemNames.Contains(item.Name);
            }
            BottomCollection.ItemsSource = bottoms;
        }

        private async void OnTopSelected(object sender, SelectionChangedEventArgs e)
        {
            selectedTop = e.CurrentSelection.FirstOrDefault() as ClothingItem;

            if (selectedTop != null && !selectedTop.IsClean)
            {
                await DisplayAlert("Unavailable", "This top is already used and not restored.", "OK");
                TopCollection.SelectedItem = null;
                selectedTop = null;
                return;
            }

            if (selectedTop != null && !string.IsNullOrEmpty(selectedTop.ImagePath))
            {
                TopPreview.Source = ImageSource.FromFile(selectedTop.ImagePath);
                CheckAndShowPreview();
            }
        }

        private async void OnBottomSelected(object sender, SelectionChangedEventArgs e)
        {
            selectedBottom = e.CurrentSelection.FirstOrDefault() as ClothingItem;

            if (selectedBottom != null && !selectedBottom.IsClean)
            {
                await DisplayAlert("Unavailable", "This bottom is already used and not restored.", "OK");
                BottomCollection.SelectedItem = null;
                selectedBottom = null;
                return;
            }

            if (selectedBottom != null && !string.IsNullOrEmpty(selectedBottom.ImagePath))
            {
                BottomPreview.Source = ImageSource.FromFile(selectedBottom.ImagePath);
                CheckAndShowPreview();
            }
        }

        private void CheckAndShowPreview()
        {
            if (selectedTop != null || selectedBottom != null)
            {
                PreviewContainer.IsVisible = true;
                OccasionSection.IsVisible = true;
            }
        }

        private async void OnEnterOccasionClicked(object sender, EventArgs e)
        {
            string occasion = await DisplayPromptAsync("Occasion", "What is the occasion for this outfit?");

            if (string.IsNullOrWhiteSpace(occasion))
            {
                await DisplayAlert("Missing Info", "Please enter an occasion.", "OK");
                return;
            }

            OccasionLabel.Text = $"Occasion: {occasion}";
            EventDatePicker.MinimumDate = DateTime.Today;
            EventDatePicker.IsVisible = true;
            ConfirmButton.IsVisible = true;
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (selectedTop == null)
            {
                await DisplayAlert("Error", "Please select a top item.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(OccasionLabel.Text))
            {
                await DisplayAlert("Missing Info", "Please enter the occasion.", "OK");
                return;
            }

            var occasion = OccasionLabel.Text.Replace("Occasion: ", "");
            var eventDate = EventDatePicker.Date;

            var outfit = new OutfitHistory
            {
                UserId = userId,
                TopItemName = selectedTop.Name,
                BottomItemName = selectedBottom?.Name,
                Occasion = occasion,
                EventDate = eventDate,
                IsReturned = false
            };

            await DatabaseService.SaveOutfitHistoryAsync(outfit);
            await DisplayAlert("Saved", "Your outfit has been added to history.", "OK");
            PreviewContainer.IsVisible = false;
            OccasionSection.IsVisible = false;
            OccasionLabel.Text = "";
            EventDatePicker.IsVisible = false;
            ConfirmButton.IsVisible = false;
            LoadUnavailableItems();
        }
    }
}
