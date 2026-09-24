using Microsoft.AspNetCore.Mvc;
using OnlineFoodOrderingSystem.Models.ViewModels;
using OnlineFoodOrderingSystem.Services;

namespace OnlineFoodOrderingSystem.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService) => _restaurantService = restaurantService;

        public async Task<IActionResult> Index(string? search, string? cuisine, string? sortBy)
        {
            var restaurants = await _restaurantService.GetAllActiveAsync(search, cuisine, sortBy);
            var vm = new RestaurantListViewModel
            {
                Restaurants = restaurants,
                SearchQuery = search,
                CuisineFilter = cuisine,
                SortBy = sortBy
            };
            return View(vm);
        }

        public async Task<IActionResult> Menu(int id, int? categoryId, bool? vegOnly, string? priceRange)
        {
            var restaurant = await _restaurantService.GetByIdAsync(id);
            if (restaurant == null) return NotFound();

            var foods = await _restaurantService.GetMenuAsync(id, categoryId, vegOnly, priceRange);
            var categories = await _restaurantService.GetCategoriesForRestaurantAsync(id);

            var vm = new RestaurantMenuViewModel
            {
                Restaurant = restaurant,
                FoodItems = foods,
                Categories = categories,
                SelectedCategoryId = categoryId,
                VegOnly = vegOnly,
                PriceRange = priceRange
            };
            return View(vm);
        }
    }
}
