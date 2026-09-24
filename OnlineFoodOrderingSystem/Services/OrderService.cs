using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;

namespace OnlineFoodOrderingSystem.Services
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(string userId, int addressId, string paymentMethod, string? couponCode, decimal discount);
        Task<List<Order>> GetOrderHistoryAsync(string userId);
        Task<Order?> GetOrderDetailsAsync(int orderId, string? userId = null);
        Task<bool> UpdateStatusAsync(int orderId, string newStatus);
        Task<List<Order>> GetAllOrdersAsync(string? statusFilter = null);
        Task<Order?> GuestTrackAsync(int orderId, string phone);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;

        public OrderService(ApplicationDbContext db, ICartService cartService, INotificationService notificationService)
        {
            _db = db;
            _cartService = cartService;
            _notificationService = notificationService;
        }

        public async Task<int> PlaceOrderAsync(string userId, int addressId, string paymentMethod, string? couponCode, decimal discount)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await _cartService.GetCartAsync(userId);
                if (!cart.Items.Any()) throw new InvalidOperationException("Cart is empty.");

                var order = new Order
                {
                    UserId = userId,
                    RestaurantId = cart.Restaurant!.RestaurantId,
                    AddressId = addressId,
                    SubTotal = cart.SubTotal,
                    GST = cart.GST,
                    DeliveryFee = cart.DeliveryFee,
                    Discount = discount,
                    CouponCode = couponCode,
                    TotalAmount = cart.SubTotal + cart.GST + cart.DeliveryFee - discount,
                    PaymentMethod = paymentMethod,
                    Status = "Pending"
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var item in cart.Items)
                {
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        FoodId = item.FoodId,
                        Quantity = item.Quantity,
                        UnitPrice = item.FoodItem!.Price
                    });
                }

                _db.Payments.Add(new Payment
                {
                    OrderId = order.OrderId,
                    Method = paymentMethod,
                    Status = paymentMethod == "COD" ? "Pending" : "Success",
                    PaidAt = paymentMethod == "COD" ? null : DateTime.UtcNow,
                    TransactionRef = paymentMethod != "COD" ? $"TXN{Guid.NewGuid().ToString()[..8].ToUpper()}" : null
                });

                _db.CartItems.RemoveRange(_db.CartItems.Where(c => c.UserId == userId));
                await _db.SaveChangesAsync();

                await _notificationService.NotifyAsync(userId,
                    $"Order #{order.OrderId:D5} placed successfully! We're getting it ready.",
                    $"/Order/Track/{order.OrderId}");

                await transaction.CommitAsync();
                return order.OrderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Order>> GetOrderHistoryAsync(string userId) =>
            await _db.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

        public async Task<Order?> GetOrderDetailsAsync(int orderId, string? userId = null)
        {
            var query = _db.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.Address)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
                .Include(o => o.Payment)
                .Where(o => o.OrderId == orderId);

            if (userId != null)
                query = query.Where(o => o.UserId == userId);

            return await query.FirstOrDefaultAsync();
        }

        // State machine: Pending→Preparing→Packed→OutForDelivery→Delivered, or any→Cancelled (from Pending/Preparing)
        public async Task<bool> UpdateStatusAsync(int orderId, string newStatus)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return false;

            var allowed = order.Status switch
            {
                "Pending"       => new[] { "Preparing", "Cancelled" },
                "Preparing"     => new[] { "Packed", "Cancelled" },
                "Packed"        => new[] { "OutForDelivery" },
                "OutForDelivery" => new[] { "Delivered" },
                _               => Array.Empty<string>()
            };

            if (!allowed.Contains(newStatus)) return false;

            order.Status = newStatus;
            await _db.SaveChangesAsync();

            var statusMsg = newStatus switch
            {
                "Preparing"     => "Your order is being prepared! 🍳",
                "Packed"        => "Your order is packed and ready!",
                "OutForDelivery" => "Your order is out for delivery! 🚴",
                "Delivered"     => "Your order has been delivered! Enjoy your meal! 🎉",
                "Cancelled"     => "Your order has been cancelled.",
                _               => $"Your order status is now: {newStatus}"
            };

            await _notificationService.NotifyAsync(order.UserId,
                $"Order #{orderId:D5}: {statusMsg}",
                $"/Order/Track/{orderId}");

            return true;
        }

        public async Task<List<Order>> GetAllOrdersAsync(string? statusFilter = null)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(o => o.Status == statusFilter);

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<Order?> GuestTrackAsync(int orderId, string phone)
        {
            return await _db.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.User!.PhoneNumber == phone);
        }
    }
}
