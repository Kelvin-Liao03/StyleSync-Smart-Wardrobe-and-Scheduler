using StyleSync.Services;
using Plugin.LocalNotification;

namespace StyleSync;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Set the main page
        MainPage = new NavigationPage(new LoginPage());

        Task.Run(async () =>
        {
            await DatabaseService.InitAsync();
        });
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // 🔔 Trigger low inventory and upcoming event notifications
        await CheckLowInventoryAsync();
        await ShowEventRemindersAsync();
    }

    private async Task CheckLowInventoryAsync()
    {
        var lowStockItems = await DatabaseService.GetLowStockItemsAsync(SessionManager.CurrentUserId);
        foreach (var item in lowStockItems)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(1000),
                Title = "Low Inventory Alert",
                Description = $"You have only {item.TimesWorn} {item.Name}(s) left.",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                }
            };
            await LocalNotificationCenter.Current.Show(request);
            
        }
    }

    private async Task ShowEventRemindersAsync()
    {
        var upcomingEvents = await DatabaseService.GetUpcomingEventsAsync(SessionManager.CurrentUserId);
        foreach (var outfit in upcomingEvents)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(1000),
                Title = "Event Reminder",
                Description = $"Upcoming: {outfit.Occasion} on {outfit.EventDate:dd MMM yyyy}",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                }
            };
            await LocalNotificationCenter.Current.Show(request);
        }
    }
}
