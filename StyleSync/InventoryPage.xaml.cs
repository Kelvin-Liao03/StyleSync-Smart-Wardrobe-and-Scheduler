using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using StyleSync.Models;
using StyleSync.Services;
using System.Threading.Tasks;

namespace StyleSync
{
    public partial class InventoryPage : ContentPage
    {
        private SQLiteAsyncConnection _database;
        public ObservableCollection<InventoryDisplayItem> DisplayItems { get; set; } = new();

        int userId = SessionManager.CurrentUserId;

        public InventoryPage()
        {
            InitializeComponent();
            InitializeDatabaseAsync();
        }

        private async void InitializeDatabaseAsync()
        {
            _database = await DatabaseService.GetConnectionAsync();
            await LoadInventory();
        }

        private async Task LoadInventory()
        {
            DisplayItems.Clear();

            // ✅ Filter by UserId so only the current user's items are shown
            var items = await _database.Table<ClothingItem>()
                                       .Where(i => i.UserId == userId)
                                       .ToListAsync();

            var grouped = items
                .GroupBy(i => i.Type)
                .Select(g => new InventoryDisplayItem
                {
                    Name = g.Key,
                    Quantity = g.Count(),
                    CleanCount = g.Count(x => x.IsClean),
                    IsAvailableText = g.All(x => x.IsAvailable) ? "All Available" : "Some Unavailable"
                });

            foreach (var display in grouped)
            {
                DisplayItems.Add(display);

                if (display.Quantity <= 1)
                {
                    await Toast.Make($"⚠️ Only {display.Quantity} {display.Name}(s) left!", ToastDuration.Short, 14).Show();
                }
            }

            inventoryList.ItemsSource = DisplayItems;
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadInventory();
        }

        public class InventoryDisplayItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public int CleanCount { get; set; }
            public string IsAvailableText { get; set; }
        }
    }
}
