using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;

namespace OnlineFoodOrderingSystem.Services
{
    public interface ICartService
    {
        Task<AddToCartResult> AddToCartAsync(string userId, int foodId, int quantity);
        Task ClearAndAddAsync(string userId, int foodId, int quantity);
        Task<CartViewModel> GetCartAsync(string userId);
        Task UpdateQuantityAsync(string userId, int foodId, int quantity);
        Task RemoveItemAsync(string userId, int foodId);
        Task ClearCartAsync(string userId);
        Task<int> GetCartCountAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;
        private const decimal GstRate = 0.05m;

        public CartService(ApplicationDbContext db) => _db = db;

        public async Task<AddToCartResult> AddToCartAsync(string userId, int foodId, int quantity)
        {
            var food = await _db.FoodItems.Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.FoodId == foodId);
            if (food == null) return new AddToCartResult { IsSuccess = false };

            var existingCart = await _db.CartItems
                .Include(c => c.FoodItem).ThenInclude(f => f!.Restaurant)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var currentRestaurantId = existingCart.FirstOrDefault()?.FoodItem?.RestaurantId;

            if (currentRestaurantId != null && currentRestaurantId != food.RestaurantId)
            {
                var existingRestaurant = existingCart.FirstOrDefault()?.FoodItem?.Restaurant;
                return AddToCartResult.RestaurantConflict(
                    currentRestaurantId.Value,
                    existingRestaurant?.Name ?? "another restaurant",
                    food.RestaurantId,
                    foodId,
                    quantity);
            }

            var existingLine = existingCart.FirstOrDefault(c => c.FoodId == foodId);
            if (existingLine != null)
                existingLine.Quantity += quantity;
            else
                _db.CartItems.Add(new CartItem { UserId = userId, FoodId = foodId, Quantity = quantity });

            await _db.SaveChangesAsync();
            return AddToCartResult.Success();
        }

        public async Task ClearAndAddAsync(string userId, int foodId, int quantity)
        {
            var existing = _db.CartItems.Where(c => c.UserId == userId);
            _db.CartItems.RemoveRange(existing);
            _db.CartItems.Add(new CartItem { UserId = userId, FoodId = foodId, Quantity = quantity });
            await _db.SaveChangesAsync();
        }

        public async Task<CartViewModel> GetCartAsync(string userId)
        {
            var items = await _db.CartItems
                .Include(c => c.FoodItem).ThenInclude(f => f!.Restaurant)
                .Include(c => c.FoodItem).ThenInclude(f => f!.Category)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var subtotal = items.Sum(i => i.Quantity * i.FoodItem!.Price);
            var gst = Math.Round(subtotal * GstRate, 2);
            var restaurant = items.FirstOrDefault()?.FoodItem?.Restaurant;
            var deliveryFee = (restaurant != null && subtotal >= restaurant.FreeDeliveryAboveAmount) ? 0 : (items.Any() ? 30m : 0m);

            return new CartViewModel
            {
                Items = items,
                Restaurant = restaurant,
                SubTotal = subtotal,
                GST = gst,
                DeliveryFee = deliveryFee
            };
        }

        public async Task UpdateQuantityAsync(string userId, int foodId, int quantity)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.FoodId == foodId);
            if (item == null) return;
            if (quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.Quantity = quantity;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(string userId, int foodId)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.FoodId == foodId);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = _db.CartItems.Where(c => c.UserId == userId);
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(string userId) =>
            await _db.CartItems.Where(c => c.UserId == userId).SumAsync(c => c.Quantity);
    }
}
