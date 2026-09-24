using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Data;
using OnlineFoodOrderingSystem.Models.Entities;
using OnlineFoodOrderingSystem.Models.ViewModels;

namespace OnlineFoodOrderingSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
            return View(addresses);
        }

        [HttpGet]
        public IActionResult Add() => View(new AddressViewModel());

        [HttpPost]
        public async Task<IActionResult> Add(AddressViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var userId = _userManager.GetUserId(User)!;

            if (model.IsDefault)
            {
                var existing = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
                existing.ForEach(a => a.IsDefault = false);
            }

            _db.Addresses.Add(new Address
            {
                UserId = userId,
                Label = model.Label,
                AddressLine = model.AddressLine,
                City = model.City,
                State = model.State,
                PinCode = model.PinCode,
                IsDefault = model.IsDefault
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Address added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
            if (addr == null) return NotFound();
            return View(new AddressViewModel
            {
                AddressId = addr.AddressId,
                Label = addr.Label,
                AddressLine = addr.AddressLine,
                City = addr.City,
                State = addr.State,
                PinCode = addr.PinCode,
                IsDefault = addr.IsDefault
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddressViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var userId = _userManager.GetUserId(User)!;
            var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.AddressId == model.AddressId && a.UserId == userId);
            if (addr == null) return NotFound();

            if (model.IsDefault)
            {
                var all = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
                all.ForEach(a => a.IsDefault = false);
            }

            addr.Label = model.Label;
            addr.AddressLine = model.AddressLine;
            addr.City = model.City;
            addr.State = model.State;
            addr.PinCode = model.PinCode;
            addr.IsDefault = model.IsDefault;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Address updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
            if (addr != null) { _db.Addresses.Remove(addr); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Address deleted.";
            return RedirectToAction("Index");
        }
    }
}
