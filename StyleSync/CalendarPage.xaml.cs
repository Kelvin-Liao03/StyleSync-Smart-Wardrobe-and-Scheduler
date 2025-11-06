using System;
using Microsoft.Maui.Controls;
using StyleSync.Services;

namespace StyleSync
{
    public partial class CalendarPage : ContentPage
    {
        int userId = SessionManager.CurrentUserId;

        public CalendarPage()
        {
            InitializeComponent();
            eventDatePicker.MaximumDate = DateTime.Today;
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            DateTime selectedDate = e.NewDate;
            eventLabel.Text = $"You selected: {selectedDate:dddd, MMMM dd, yyyy}";
        }
    }
}
