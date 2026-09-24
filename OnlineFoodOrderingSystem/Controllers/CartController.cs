using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int foodId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.AddToCartAsync(userId, foodId, quantity);

            if (result.IsConflict)
            {
                return Json(new
                {
                    conflict = true,
                    existingRestaurantName = result.ExistingRestaurantName,
                    foodId,
                    quantity
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ClearAndAdd(int foodId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            await _cartService.ClearAndAddAsync(userId, foodId, quantity);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int foodId, int quantity)
        {
            var userId = _userManager.GetUserId(User)!;
            await _cartService.UpdateQuantityAsync(userId, foodId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int foodId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _cartService.RemoveItemAsync(userId, foodId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User)!;
            await _cartService.ClearCartAsync(userId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var userId = _userManager.GetUserId(User)!;
            var count = await _cartService.GetCartCountAsync(userId);
            return Json(new { count });
        }
    }
}
