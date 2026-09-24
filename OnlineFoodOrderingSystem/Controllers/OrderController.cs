using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _orderService.GetOrderHistoryAsync(userId);
            return View(orders);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Track(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null) return NotFound();
            var vm = new OrderSummaryViewModel { Order = order, Items = order.OrderItems.ToList() };
            return View(vm);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null) return NotFound();
            return View(new OrderSummaryViewModel { Order = order, Items = order.OrderItems.ToList() });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Reorder(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null) return NotFound();

            // Clear cart and add all items from old order
            await _cartService.ClearCartAsync(userId);
            foreach (var item in order.OrderItems)
            {
                await _cartService.AddToCartAsync(userId, item.FoodId, item.Quantity);
            }
            return RedirectToAction("Index", "Cart");
        }

        [HttpGet]
        public IActionResult GuestTrack() => View();

        [HttpPost]
        public async Task<IActionResult> GuestTrack(int orderId, string phone)
        {
            var order = await _orderService.GuestTrackAsync(orderId, phone);
            if (order == null)
            {
                ViewBag.Error = "Order not found. Please check your Order ID and phone number.";
                return View();
            }
            var vm = new OrderSummaryViewModel { Order = order, Items = order.OrderItems.ToList() };
            return View("Track", vm);
        }
    }
}
