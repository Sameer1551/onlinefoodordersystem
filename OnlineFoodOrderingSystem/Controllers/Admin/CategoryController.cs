using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CategoryController(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        public async Task<IActionResult> Index() => View(await _db.Categories.OrderBy(c => c.Name).ToListAsync());

        [HttpGet] public IActionResult Create() => View(new Category());

        [HttpPost]
        public async Task<IActionResult> Create(Category model, IFormFile? imageFile)
        {
            ModelState.Remove("FoodItems");
            if (!ModelState.IsValid) return View(model);
            model.ImageUrl = await SaveImageAsync(imageFile) ?? model.ImageUrl;
            _db.Categories.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) => View(await _db.Categories.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> Edit(Category model, IFormFile? imageFile)
        {
            ModelState.Remove("FoodItems");
            if (!ModelState.IsValid) return View(model);
            var c = await _db.Categories.FindAsync(model.CategoryId);
            if (c == null) return NotFound();
            c.Name = model.Name;
            c.ImageUrl = await SaveImageAsync(imageFile) ?? c.ImageUrl;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c != null) { _db.Categories.Remove(c); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var dir = Path.Combine(_env.WebRootPath, "images", "categories");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            await using var stream = System.IO.File.Create(Path.Combine(dir, fileName));
            await file.CopyToAsync(stream);
            return $"/images/categories/{fileName}";
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext db, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _db = db; _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _db.Users.ToListAsync();
            var result = new List<(ApplicationUser User, string Role)>();
            foreach (var u in customers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add((u, string.Join(", ", roles)));
            }
            ViewBag.CustomerRoles = result.ToDictionary(x => x.User.Id, x => x.Role);
            return View(customers);
        }
    }
}
