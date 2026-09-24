using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _db.Wishlists
                .Include(w => w.FoodItem).ThenInclude(f => f!.Restaurant)
                .Include(w => w.FoodItem).ThenInclude(f => f!.Category)
                .Where(w => w.UserId == userId)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int foodId)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _db.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.FoodId == foodId);
            bool added;
            if (existing != null)
            {
                _db.Wishlists.Remove(existing);
                added = false;
            }
            else
            {
                _db.Wishlists.Add(new Wishlist { UserId = userId, FoodId = foodId });
                added = true;
            }
            await _db.SaveChangesAsync();
            return Json(new { added });
        }
    }

    [Authorize(Roles = "Customer")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public NotificationController(INotificationService notificationService, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _notificationService.GetAllAsync(userId);
            await _notificationService.MarkAllReadAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead()
        {
            var userId = _userManager.GetUserId(User)!;
            await _notificationService.MarkAllReadAsync(userId);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var userId = _userManager.GetUserId(User)!;
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }
    }
}
