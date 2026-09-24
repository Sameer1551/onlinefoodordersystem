using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ICouponService _couponService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public CheckoutController(ICartService cartService, IOrderService orderService,
            ICouponService couponService, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _cartService = cartService;
            _orderService = orderService;
            _couponService = couponService;
            _userManager = userManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(userId);
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

            var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var vm = new CheckoutViewModel { Cart = cart, Addresses = addresses };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var userId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(userId);
            var (valid, msg, discount) = await _couponService.ValidateAsync(couponCode, cart.SubTotal, cart.Restaurant?.RestaurantId);

            var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var vm = new CheckoutViewModel
            {
                Cart = cart,
                Addresses = addresses,
                CouponCode = valid ? couponCode : null,
                CouponMessage = msg,
                Discount = discount
            };
            return View("Index", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int addressId, string paymentMethod, string? couponCode, decimal discount)
        {
            var userId = _userManager.GetUserId(User)!;
            try
            {
                var orderId = await _orderService.PlaceOrderAsync(userId, addressId, paymentMethod, couponCode, discount);
                return RedirectToAction("Success", new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
            if (order == null) return NotFound();
            return View(new OrderSummaryViewModel { Order = order, Items = order.OrderItems.ToList() });
        }
    }
}
