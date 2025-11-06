using SQLite;

namespace StyleSync.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Username { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Gender { get; set; }
        public string UsualEvents { get; set; }
        public string ClothingStyles { get; set; }
        public string ShoppingFrequency { get; set; }
    }

}
