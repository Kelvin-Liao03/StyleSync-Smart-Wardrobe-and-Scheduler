using System;
using Microsoft.Maui.Controls;
using StyleSync.Models;
using StyleSync.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace StyleSync
{
    public partial class RestoreOutfitsPage : ContentPage
    {
        int userId = SessionManager.CurrentUserId;
        private List<OutfitHistory> outfitList = new();
        private List<OutfitHistory> selectedForClear = new();

        public RestoreOutfitsPage()
        {
            InitializeComponent();
            LoadOutfitHistory();
        }

        private async void LoadOutfitHistory()
        {
            outfitList = await DatabaseService.GetOutfitHistoryByUserAsync(userId);

            foreach (var outfit in outfitList)
            {
                outfit.TopImage = await GetImageForItemAsync(outfit.TopItemName);
                outfit.BottomImage = await GetImageForItemAsync(outfit.BottomItemName);
            }

            outfitCollection.ItemsSource = null;
            outfitCollection.ItemsSource = outfitList;
        }

        private async Task<string> GetImageForItemAsync(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName)) return string.Empty;
            var item = await DatabaseService.GetItemByNameForUserAsync(itemName, userId);
            return item?.ImagePath ?? string.Empty;
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var outfit = (OutfitHistory)button.BindingContext;

            await RestoreClothingItemsAsync(outfit);
            outfit.IsReturned = true;
            await DatabaseService.UpdateOutfitHistoryAsync(outfit);

            LoadOutfitHistory();
            await DisplayAlert("Restored", "Outfit has been returned to wardrobe.", "OK");
        }

        private async Task RestoreClothingItemsAsync(OutfitHistory outfit)
        {
            if (!string.IsNullOrEmpty(outfit.TopItemName))
            {
                var topItem = await DatabaseService.GetItemByNameForUserAsync(outfit.TopItemName, userId);
                if (topItem != null)
                {
                    topItem.IsAvailable = true;
                    topItem.IsClean = true;
                    await DatabaseService.SaveItemAsync(topItem);
                }
            }

            if (!string.IsNullOrEmpty(outfit.BottomItemName))
            {
                var bottomItem = await DatabaseService.GetItemByNameForUserAsync(outfit.BottomItemName, userId);
                if (bottomItem != null)
                {
                    bottomItem.IsAvailable = true;
                    bottomItem.IsClean = true;
                    await DatabaseService.SaveItemAsync(bottomItem);
                }
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var outfit = (OutfitHistory)button.BindingContext;

            bool confirm = await DisplayAlert("Confirm Cancel", "Are you sure you want to cancel this scheduled outfit?", "Yes", "No");
            if (confirm)
            {
                await DatabaseService.DeleteOutfitHistoryAsync(outfit);
                LoadOutfitHistory();
                await DisplayAlert("Cancelled", "Scheduled outfit has been removed.", "OK");
            }
        }

        private void OnSelectForClearTapped(object sender, EventArgs e)
        {
            var grid = (Grid)sender;
            var tappedOutfit = (OutfitHistory)grid.BindingContext;

            if (selectedForClear.Contains(tappedOutfit))
            {
                selectedForClear.Remove(tappedOutfit);
                grid.BackgroundColor = Colors.White;
            }
            else
            {
                selectedForClear.Add(tappedOutfit);
                grid.BackgroundColor = Colors.LightGray;
            }
        }

        private async void OnClearSelectedClicked(object sender, EventArgs e)
        {
            if (selectedForClear.Count == 0)
            {
                await DisplayAlert("No Selection", "Please select outfits to clear.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {selectedForClear.Count} outfits?", "Yes", "No");
            if (confirm)
            {
                foreach (var outfit in selectedForClear)
                {
                    await DatabaseService.DeleteOutfitHistoryAsync(outfit);
                }
                selectedForClear.Clear();
                LoadOutfitHistory();
                await DisplayAlert("Cleared", "Selected outfits have been deleted.", "OK");
            }
        }

        private async void OnEditDateClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var outfit = (OutfitHistory)button.BindingContext;

            var newDate = await DisplayPromptAsync("Edit Event Date",
                                                   "Enter new date (yyyy-MM-dd):",
                                                   initialValue: outfit.EventDate.ToString("yyyy-MM-dd"));

            if (!string.IsNullOrEmpty(newDate) && DateTime.TryParse(newDate, out var parsedDate))
            {
                outfit.EventDate = parsedDate;
                await DatabaseService.UpdateOutfitHistoryAsync(outfit);
                LoadOutfitHistory();
                await DisplayAlert("Updated", "Event date has been updated.", "OK");
            }
        }

    }
}
