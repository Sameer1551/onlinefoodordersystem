using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public FoodController(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        public async Task<IActionResult> Index(int? restaurantId)
        {
            var query = _db.FoodItems.Include(f => f.Restaurant).Include(f => f.Category).AsQueryable();
            if (restaurantId.HasValue) query = query.Where(f => f.RestaurantId == restaurantId.Value);
            ViewBag.Restaurants = await _db.Restaurants.Where(r => r.IsActive).ToListAsync();
            ViewBag.SelectedRestaurantId = restaurantId;
            return View(await query.OrderBy(f => f.Name).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Restaurants = new SelectList(await _db.Restaurants.Where(r => r.IsActive).ToListAsync(), "RestaurantId", "Name");
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "Name");
            return View(new FoodItem());
        }

        [HttpPost]
        public async Task<IActionResult> Create(FoodItem model, IFormFile? imageFile)
        {
            ModelState.Remove("Restaurant"); ModelState.Remove("Category");
            ModelState.Remove("CartItems"); ModelState.Remove("OrderItems");
            ModelState.Remove("Wishlists"); ModelState.Remove("Reviews");
            if (!ModelState.IsValid)
            {
                ViewBag.Restaurants = new SelectList(await _db.Restaurants.Where(r => r.IsActive).ToListAsync(), "RestaurantId", "Name");
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "Name");
                return View(model);
            }
            model.ImageUrl = await SaveImageAsync(imageFile) ?? "/images/foods/default.jpg";
            _db.FoodItems.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Food item added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var f = await _db.FoodItems.FindAsync(id);
            if (f == null) return NotFound();
            ViewBag.Restaurants = new SelectList(await _db.Restaurants.Where(r => r.IsActive).ToListAsync(), "RestaurantId", "Name", f.RestaurantId);
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "Name", f.CategoryId);
            return View(f);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FoodItem model, IFormFile? imageFile)
        {
            ModelState.Remove("Restaurant"); ModelState.Remove("Category");
            ModelState.Remove("CartItems"); ModelState.Remove("OrderItems");
            ModelState.Remove("Wishlists"); ModelState.Remove("Reviews");
            if (!ModelState.IsValid)
            {
                ViewBag.Restaurants = new SelectList(await _db.Restaurants.Where(r => r.IsActive).ToListAsync(), "RestaurantId", "Name");
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "Name");
                return View(model);
            }
            var f = await _db.FoodItems.FindAsync(model.FoodId);
            if (f == null) return NotFound();

            f.Name = model.Name; f.Description = model.Description;
            f.Price = model.Price; f.IsVeg = model.IsVeg;
            f.IsAvailable = model.IsAvailable; f.RestaurantId = model.RestaurantId;
            f.CategoryId = model.CategoryId;
            f.ImageUrl = await SaveImageAsync(imageFile) ?? f.ImageUrl;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Food item updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _db.FoodItems.FindAsync(id);
            if (f != null) { _db.FoodItems.Remove(f); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Food item deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var f = await _db.FoodItems.FindAsync(id);
            if (f != null) { f.IsAvailable = !f.IsAvailable; await _db.SaveChangesAsync(); }
            return Json(new { isAvailable = f?.IsAvailable });
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var dir = Path.Combine(_env.WebRootPath, "images", "foods");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            await using var stream = System.IO.File.Create(Path.Combine(dir, fileName));
            await file.CopyToAsync(stream);
            return $"/images/foods/{fileName}";
        }
    }
}
