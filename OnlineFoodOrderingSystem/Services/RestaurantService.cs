using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;

namespace OnlineFoodOrderingSystem.Services
{
    public interface IRestaurantService
    {
        Task<List<Restaurant>> GetAllActiveAsync(string? search = null, string? cuisine = null, string? sortBy = null);
        Task<Restaurant?> GetByIdAsync(int id);
        Task<List<FoodItem>> GetMenuAsync(int restaurantId, int? categoryId = null, bool? vegOnly = null, string? priceRange = null);
        Task<List<Category>> GetCategoriesForRestaurantAsync(int restaurantId);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _db;
        public RestaurantService(ApplicationDbContext db) => _db = db;

        public async Task<List<Restaurant>> GetAllActiveAsync(string? search = null, string? cuisine = null, string? sortBy = null)
        {
            var query = _db.Restaurants.Where(r => r.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.Name.Contains(search) || r.CuisineTags!.Contains(search));

            if (!string.IsNullOrWhiteSpace(cuisine))
                query = query.Where(r => r.CuisineTags!.Contains(cuisine));

            query = sortBy switch
            {
                "rating" => query.OrderByDescending(r => r.AvgRating),
                "delivery" => query.OrderBy(r => r.DeliveryTimeMinMinutes),
                "name" => query.OrderBy(r => r.Name),
                _ => query.OrderByDescending(r => r.RatingCount)
            };

            return await query.ToListAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(int id) =>
            await _db.Restaurants.FirstOrDefaultAsync(r => r.RestaurantId == id && r.IsActive);

        public async Task<List<FoodItem>> GetMenuAsync(int restaurantId, int? categoryId = null, bool? vegOnly = null, string? priceRange = null)
        {
            var query = _db.FoodItems
                .Include(f => f.Category)
                .Where(f => f.RestaurantId == restaurantId && f.IsAvailable);

            if (categoryId.HasValue)
                query = query.Where(f => f.CategoryId == categoryId.Value);

            if (vegOnly == true)
                query = query.Where(f => f.IsVeg);

            if (!string.IsNullOrEmpty(priceRange))
            {
                query = priceRange switch
                {
                    "0-200" => query.Where(f => f.Price <= 200),
                    "200-400" => query.Where(f => f.Price > 200 && f.Price <= 400),
                    "400-600" => query.Where(f => f.Price > 400 && f.Price <= 600),
                    "600+" => query.Where(f => f.Price > 600),
                    _ => query
                };
            }

            return await query.OrderBy(f => f.CategoryId).ThenBy(f => f.Name).ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesForRestaurantAsync(int restaurantId) =>
            await _db.FoodItems
                .Where(f => f.RestaurantId == restaurantId && f.IsAvailable)
                .Select(f => f.Category!)
                .Distinct()
                .OrderBy(c => c.Name)
                .ToListAsync();
    }
}
