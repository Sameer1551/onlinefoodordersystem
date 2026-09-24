using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Services
{
    public interface ICouponService
    {
        Task<(bool IsValid, string Message, decimal Discount)> ValidateAsync(string code, decimal subtotal, int? restaurantId = null);
    }

    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _db;
        public CouponService(ApplicationDbContext db) => _db = db;

        public async Task<(bool IsValid, string Message, decimal Discount)> ValidateAsync(string code, decimal subtotal, int? restaurantId = null)
        {
            var coupon = await _db.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

            if (coupon == null)
                return (false, "Invalid coupon code.", 0);

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
                return (false, "This coupon has expired.", 0);

            if (subtotal < coupon.MinOrderValue)
                return (false, $"Minimum order value of ₹{coupon.MinOrderValue} required.", 0);

            if (coupon.RestaurantId.HasValue && coupon.RestaurantId != restaurantId)
                return (false, "This coupon is not valid for this restaurant.", 0);

            var discount = coupon.DiscountType == "Percentage"
                ? Math.Round(subtotal * coupon.DiscountValue / 100, 2)
                : coupon.DiscountValue;

            // Cap percentage discount at reasonable max
            if (coupon.DiscountType == "Percentage" && discount > subtotal)
                discount = subtotal;

            return (true, $"Coupon applied! You save ₹{discount}", discount);
        }
    }

    public interface INotificationService
    {
        Task NotifyAsync(string userId, string message, string? link = null);
        Task<List<Notification>> GetUnreadAsync(string userId);
        Task<List<Notification>> GetAllAsync(string userId);
        Task MarkAllReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        public NotificationService(ApplicationDbContext db) => _db = db;

        public async Task NotifyAsync(string userId, string message, string? link = null)
        {
            _db.Notifications.Add(new Notification { UserId = userId, Message = message, Link = link });
            await _db.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadAsync(string userId) =>
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<List<Notification>> GetAllAsync(string userId) =>
            await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

        public async Task MarkAllReadAsync(string userId)
        {
            var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            unread.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId) =>
            await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public interface IReportService
    {
        Task<AdminDashboardSummary> GetDashboardSummaryAsync();
        Task<List<DailySalesData>> GetMonthlySalesAsync(int year, int month);
        Task<List<TopFoodData>> GetTopFoodsAsync(int top = 10);
    }

    public class AdminDashboardSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalRestaurants { get; set; }
        public int PendingOrders { get; set; }
    }

    public class DailySalesData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class TopFoodData
    {
        public string FoodName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public int TotalOrdered { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _db;
        public ReportService(ApplicationDbContext db) => _db = db;

        public async Task<AdminDashboardSummary> GetDashboardSummaryAsync() => new AdminDashboardSummary
        {
            TotalOrders = await _db.Orders.CountAsync(),
            TotalRevenue = await _db.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            TotalCustomers = await _db.Users.CountAsync(),
            TotalRestaurants = await _db.Restaurants.CountAsync(r => r.IsActive),
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending")
        };

        public async Task<List<DailySalesData>> GetMonthlySalesAsync(int year, int month)
        {
            var orders = await _db.Orders
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month && o.Status != "Cancelled")
                .ToListAsync();

            return orders
                .GroupBy(o => o.OrderDate.Day)
                .Select(g => new DailySalesData
                {
                    Date = $"{year}-{month:D2}-{g.Key:D2}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        public async Task<List<TopFoodData>> GetTopFoodsAsync(int top = 10) =>
            await _db.OrderItems
                .Include(oi => oi.FoodItem).ThenInclude(f => f!.Restaurant)
                .GroupBy(oi => new { oi.FoodId, oi.FoodItem!.Name, RestaurantName = oi.FoodItem.Restaurant!.Name })
                .Select(g => new TopFoodData
                {
                    FoodName = g.Key.Name,
                    RestaurantName = g.Key.RestaurantName,
                    TotalOrdered = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(t => t.TotalOrdered)
                .Take(top)
                .ToListAsync();
    }
}
