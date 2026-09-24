using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _db;

        public OrderController(IOrderService orderService, ApplicationDbContext db) { _orderService = orderService; _db = db; }

        public async Task<IActionResult> Index(string? status)
        {
            var orders = await _orderService.GetAllOrdersAsync(status);
            ViewBag.StatusFilter = status ?? "All";
            ViewBag.AllCount = await _db.Orders.CountAsync();
            ViewBag.PendingCount = await _db.Orders.CountAsync(o => o.Status == "Pending");
            ViewBag.PreparingCount = await _db.Orders.CountAsync(o => o.Status == "Preparing");
            ViewBag.OutForDeliveryCount = await _db.Orders.CountAsync(o => o.Status == "OutForDelivery");
            ViewBag.DeliveredCount = await _db.Orders.CountAsync(o => o.Status == "Delivered");
            ViewBag.CancelledCount = await _db.Orders.CountAsync(o => o.Status == "Cancelled");
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.Address)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var success = await _orderService.UpdateStatusAsync(orderId, status);
            TempData[success ? "Success" : "Error"] = success ? $"Order #{orderId} status updated to {status}!" : "Invalid status transition.";
            return RedirectToAction("Index");
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() => View(await _db.Coupons.Include(c => c.Restaurant).OrderByDescending(c => c.CouponId).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Restaurants = await _db.Restaurants.Where(r => r.IsActive).ToListAsync();
            return View(new Coupon());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Coupon model)
        {
            ModelState.Remove("Restaurant");
            if (!ModelState.IsValid)
            {
                ViewBag.Restaurants = await _db.Restaurants.Where(r => r.IsActive).ToListAsync();
                return View(model);
            }
            model.Code = model.Code.ToUpper();
            _db.Coupons.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Coupon created!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c != null) { _db.Coupons.Remove(c); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Coupon deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c != null) { c.IsActive = !c.IsActive; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService) => _reportService = reportService;

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var now = DateTime.UtcNow;
            year ??= now.Year;
            month ??= now.Month;
            var sales = await _reportService.GetMonthlySalesAsync(year.Value, month.Value);
            var topFoods = await _reportService.GetTopFoodsAsync(10);
            ViewBag.Sales = sales;
            ViewBag.TopFoods = topFoods;
            ViewBag.Year = year;
            ViewBag.Month = month;
            return View();
        }
    }
}
