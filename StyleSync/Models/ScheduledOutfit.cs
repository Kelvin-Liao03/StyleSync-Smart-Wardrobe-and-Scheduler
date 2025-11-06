#nullable enable

using SQLite;
using System;

namespace StyleSync.Models
{
    public class ScheduledOutfit
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // 🔑 Link to the user who scheduled the outfit
        public int UserId { get; set; }

        public string TopItemName { get; set; } = string.Empty;      // Top clothing item
        public string? BottomItemName { get; set; }                  // Optional bottom
        public string Occasion { get; set; } = string.Empty;         // e.g., Interview, Wedding
        public DateTime ScheduledDate { get; set; }                  // Date to wear the outfit
    }
}
