using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RestaurantController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public RestaurantController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index() => View(await _db.Restaurants.OrderBy(r => r.Name).ToListAsync());

        [HttpGet] public IActionResult Create() => View(new Restaurant());

        [HttpPost]
        public async Task<IActionResult> Create(Restaurant model, IFormFile? logoFile, IFormFile? bannerFile)
        {
            ModelState.Remove("FoodItems"); ModelState.Remove("Orders");
            if (!ModelState.IsValid) return View(model);

            model.LogoUrl = await SaveImageAsync(logoFile, "restaurants") ?? model.LogoUrl;
            model.BannerUrl = await SaveImageAsync(bannerFile, "restaurants") ?? model.BannerUrl;
            _db.Restaurants.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Restaurant added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _db.Restaurants.FindAsync(id);
            return r == null ? NotFound() : View(r);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Restaurant model, IFormFile? logoFile, IFormFile? bannerFile)
        {
            ModelState.Remove("FoodItems"); ModelState.Remove("Orders");
            if (!ModelState.IsValid) return View(model);

            var r = await _db.Restaurants.FindAsync(model.RestaurantId);
            if (r == null) return NotFound();

            r.Name = model.Name; r.Description = model.Description;
            r.CuisineTags = model.CuisineTags; r.City = model.City;
            r.Address = model.Address; r.DeliveryTimeMinMinutes = model.DeliveryTimeMinMinutes;
            r.DeliveryTimeMaxMinutes = model.DeliveryTimeMaxMinutes;
            r.FreeDeliveryAboveAmount = model.FreeDeliveryAboveAmount; r.IsActive = model.IsActive;
            r.LogoUrl = await SaveImageAsync(logoFile, "restaurants") ?? r.LogoUrl;
            r.BannerUrl = await SaveImageAsync(bannerFile, "restaurants") ?? r.BannerUrl;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Restaurant updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.Restaurants.FindAsync(id);
            if (r != null) { r.IsActive = false; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Restaurant deactivated.";
            return RedirectToAction("Index");
        }

        private async Task<string?> SaveImageAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;
            var dir = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(dir, fileName);
            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return $"/images/{folder}/{fileName}";
        }
    }
}
