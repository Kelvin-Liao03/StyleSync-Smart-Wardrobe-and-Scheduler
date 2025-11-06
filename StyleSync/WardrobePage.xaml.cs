using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using StyleSync.Models;
using StyleSync.Services;

namespace StyleSync
{
    public partial class WardrobePage : ContentPage
    {
        int userId = SessionManager.CurrentUserId;

        public WardrobePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadClothingItems();
        }

        private async Task LoadClothingItems()
        {
            var items = await DatabaseService.GetItemsByUserAsync(userId); // ✅ Only fetch user's items
            wardrobeList.ItemsSource = items;
        }

        private async void OnAddClothingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddClothingPage());
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            var view = (VisualElement)sender;
            var item = (ClothingItem)view.BindingContext;

            await Navigation.PushAsync(new EditClothingPage(item));
        }
    }
}
