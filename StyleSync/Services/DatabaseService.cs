#nullable enable

using SQLite;
using StyleSync.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace StyleSync.Services
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; set; }
    }

    public class DatabaseService
    {
        private static SQLiteAsyncConnection? _database;

        public static async Task InitAsync()
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "stylesync.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<ClothingItem>();
            await _database.CreateTableAsync<OutfitHistory>();
            await _database.CreateTableAsync<ScheduledOutfit>();
            await _database.CreateTableAsync<User>();
        }

        public static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database == null)
                await InitAsync();

            return _database ?? throw new InvalidOperationException("Database not initialized.");
        }

        public static async Task<List<ClothingItem>> GetClothingItemsByUserAsync()
        {
            return await _database.Table<ClothingItem>()
                .Where(i => i.UserId == SessionManager.CurrentUserId)
                .ToListAsync();
        }

        public static Task<int> SaveItemAsync(ClothingItem item) =>
            item.Id != 0 ? _database.UpdateAsync(item) : _database.InsertAsync(item);

        public static Task<int> DeleteItemAsync(ClothingItem item) =>
            _database.DeleteAsync(item);

        public static async Task<Dictionary<string, int>> GetCleanInventoryCountsAsync()
        {
            var items = await _database.Table<ClothingItem>()
                .Where(i => i.UserId == SessionManager.CurrentUserId && i.IsClean && i.IsAvailable)
                .ToListAsync();

            return items
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public static async Task<ClothingItem?> GetItemByNameAsync(string name)
        {
            var db = await GetConnectionAsync();
            return await db.Table<ClothingItem>()
                .Where(i => i.UserId == SessionManager.CurrentUserId && i.Name == name)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<ClothingItem>> GetTopItemsAsync()
        {
            var topTypes = new List<string>
            {
                "T-shirt", "Shirt", "Blouse", "Tank Top", "Hoodie",
                "Sweater", "Cardigan", "Coat", "Jacket"
            };

            var allItems = await GetClothingItemsByUserAsync();
            return allItems.Where(item => topTypes.Contains(item.Type)).ToList();
        }

        public static async Task<List<ClothingItem>> GetBottomItemsAsync()
        {
            var bottomTypes = new List<string>
            {
                "Pants", "Jeans", "Shorts", "Skirt"
            };

            var allItems = await GetClothingItemsByUserAsync();
            return allItems.Where(item => bottomTypes.Contains(item.Type)).ToList();
        }

        public static async Task<List<OutfitHistory>> GetOutfitHistoryByUserAsync()
        {
            return await _database.Table<OutfitHistory>()
                .Where(h => h.UserId == SessionManager.CurrentUserId)
                .OrderByDescending(h => h.EventDate)
                .ToListAsync();
        }

        public static Task<int> SaveOutfitHistoryAsync(OutfitHistory history) =>
            _database.InsertAsync(history);

        public static Task<int> UpdateOutfitHistoryAsync(OutfitHistory history) =>
            _database.UpdateAsync(history);

        public static Task<int> DeleteOutfitHistoryAsync(OutfitHistory history) =>
            _database.DeleteAsync(history);

        public static async Task<int> SaveScheduledOutfitAsync(ScheduledOutfit outfit)
        {
            return await _database.InsertAsync(outfit);
        }

        public static async Task<List<ScheduledOutfit>> GetAllScheduledOutfitsByUserAsync()
        {
            return await _database.Table<ScheduledOutfit>()
                .Where(o => o.UserId == SessionManager.CurrentUserId)
                .ToListAsync();
        }

        public static async Task<List<ScheduledOutfit>> GetScheduledOutfitsByDateAsync(DateTime date)
        {
            return await _database.Table<ScheduledOutfit>()
                .Where(o => o.UserId == SessionManager.CurrentUserId && o.ScheduledDate == date)
                .ToListAsync();
        }

        public static Task<int> DeleteScheduledOutfitAsync(ScheduledOutfit outfit) =>
            _database.DeleteAsync(outfit);

        public async Task<User> GetUserAsync(string username, string password)
        {
            return await _database.Table<User>()
                .Where(u => u.Username == username && u.Password == password)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _database.Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            return await _database.UpdateAsync(user);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            var user = await _database.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            return user != null;
        }

        public static async Task<List<ClothingItem>> GetLowStockItemsAsync(int userId)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<ClothingItem>()
                           .Where(item => item.UserId == userId && (item.TimesWorn >= 3 || !item.IsClean))
                           .ToListAsync();
        }

        public static async Task<List<OutfitHistory>> GetUpcomingEventsAsync(int userId)
        {
            var db = await GetDatabaseAsync();
            return await db.Table<OutfitHistory>()
                           .Where(o => o.UserId == userId && !o.IsReturned && o.EventDate >= DateTime.Today)
                           .ToListAsync();
        }



        public async Task<int> AddUserAsync(User user)
        {
            return await _database.InsertAsync(user);
        }

        public static async Task ResetDatabaseAsync()
        {
            await _database.DropTableAsync<ClothingItem>();
            await _database.CreateTableAsync<ClothingItem>();
        }

        public static async Task<List<ClothingItem>> GetItemsByUserIdAsync(int userId)
        {
            await InitAsync();
            return await _database.Table<ClothingItem>()
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public static Task<List<OutfitHistory>> GetOutfitHistoryByUserAsync(int userId) =>
            _database.Table<OutfitHistory>()
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.EventDate)
                .ToListAsync();

        public static Task<ClothingItem?> GetItemByNameForUserAsync(string name, int userId) =>
            _database.Table<ClothingItem>()
                .Where(i => i.Name == name && i.UserId == userId)
                .FirstOrDefaultAsync();

        public static async Task<List<ClothingItem>> GetTopItemsByUserAsync(int userId)
        {
            var topTypes = new[]
            {
                "T-shirt", "Shirt", "Blouse", "Tank Top", "Hoodie",
                "Sweater", "Cardigan", "Coat", "Jacket"
            };

            return await _database.Table<ClothingItem>()
                .Where(i => i.UserId == userId && topTypes.Contains(i.Type))
                .ToListAsync();
        }
        private static async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (_database == null)
                await InitAsync();

            return _database!;
        }

        public static async Task<List<ClothingItem>> GetBottomItemsByUserAsync(int userId)
        {
            var bottomTypes = new[]
            {
                "Pants", "Jeans", "Shorts", "Skirt"
            };

            return await _database.Table<ClothingItem>()
                .Where(i => i.UserId == userId && bottomTypes.Contains(i.Type))
                .ToListAsync();
        }

        public static Task<List<ClothingItem>> GetItemsByUserAsync(int userId) =>
            _database.Table<ClothingItem>()
                .Where(i => i.UserId == userId)
                .ToListAsync();
    }
}
