using System;
using SQLite;

namespace StyleSync.Models
{
    public class ClothingItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // 🔑 Foreign key to identify who owns the clothing
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public bool IsClean { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? LastWornDate { get; set; }
        public string SuitableOccasion { get; set; }
        public string WeatherSuitability { get; set; }
        public string ImagePath { get; set; }
        public int TimesWorn { get; set; }
    }
}
