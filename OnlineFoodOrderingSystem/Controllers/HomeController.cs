using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IRestaurantService _restaurantService;

        public HomeController(ApplicationDbContext db, IRestaurantService restaurantService)
        {
            _db = db;
            _restaurantService = restaurantService;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var categories = await _db.Categories.ToListAsync();
            var restaurants = await _restaurantService.GetAllActiveAsync(search: q);
            ViewBag.Categories = categories;
            ViewBag.SearchQuery = q;
            return View(restaurants);
        }

        public async Task<IActionResult> Search(string q)
        {
            var restaurants = await _restaurantService.GetAllActiveAsync(search: q);
            var categories = await _db.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SearchQuery = q;
            return View("Index", restaurants);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
