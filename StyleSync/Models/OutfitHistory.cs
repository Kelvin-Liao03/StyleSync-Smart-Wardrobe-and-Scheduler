#nullable enable

using SQLite;
using System;

namespace StyleSync.Models
{
    public class OutfitHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // 🔑 Link to the user who wore the outfit
        public int UserId { get; set; }

        public string TopItemName { get; set; } = string.Empty;
        public string? BottomItemName { get; set; }
        public string Occasion { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public bool IsReturned { get; set; }

        // 🖼️ UI-computed properties
        [Ignore]
        public string? TopImage { get; set; }

        [Ignore]
        public string? BottomImage { get; set; }

        [Ignore]
        public bool HasBottom => !string.IsNullOrEmpty(BottomItemName);

        [Ignore]
        public string EventDateFormatted => EventDate.ToString("dd MMM yyyy");

        [Ignore]
        public string Status
        {
            get
            {
                if (IsReturned)
                    return "Returned";
                if (EventDate > DateTime.Now)
                    return "Scheduled";
                return "Worn";
            }
        }

        [Ignore]
        public bool IsWorn => !IsReturned && EventDate <= DateTime.Now;

        [Ignore]
        public bool IsScheduledAndNotReturned => !IsReturned && EventDate > DateTime.Now;

        [Ignore]
        public bool IsEditableDate => EventDate > DateTime.Now && !IsReturned;
    }
}
