using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFoodOrderingSystem.Models.ViewModels;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IReportService _reportService;

        public DashboardController(IReportService reportService) => _reportService = reportService;

        public async Task<IActionResult> Index()
        {
            var summary = await _reportService.GetDashboardSummaryAsync();
            var now = DateTime.UtcNow;
            var sales = await _reportService.GetMonthlySalesAsync(now.Year, now.Month);
            var topFoods = await _reportService.GetTopFoodsAsync(5);

            var vm = new AdminDashboardViewModel
            {
                TotalOrders = summary.TotalOrders,
                TotalRevenue = summary.TotalRevenue,
                TotalCustomers = summary.TotalCustomers,
                TotalRestaurants = summary.TotalRestaurants,
                PendingOrders = summary.PendingOrders,
                MonthlySales = sales.Select(s => new DailySales { Date = s.Date, Revenue = s.Revenue, Orders = s.Orders }).ToList(),
                TopFoods = topFoods.Select(t => new TopFoodItem { FoodName = t.FoodName, RestaurantName = t.RestaurantName, TotalOrdered = t.TotalOrdered, TotalRevenue = t.TotalRevenue }).ToList()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SalesData(int year, int month)
        {
            var sales = await _reportService.GetMonthlySalesAsync(year, month);
            return Json(sales);
        }
    }
}
