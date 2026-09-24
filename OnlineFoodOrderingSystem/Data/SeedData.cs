using Microsoft.AspNetCore.Identity;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure roles exist
            foreach (var role in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin user
            const string adminEmail = "admin@foodie.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Platform Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed demo customer
            const string customerEmail = "customer@foodie.com";
            if (await userManager.FindByEmailAsync(customerEmail) == null)
            {
                var customer = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Rahul Sharma",
                    Phone = "9876543210",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(customer, "Customer@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(customer, "Customer");
            }

            // Seed categories
            if (!db.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Pizza", ImageUrl = "/images/categories/pizza.png" },
                    new() { Name = "Burger", ImageUrl = "/images/categories/burger.png" },
                    new() { Name = "Chinese", ImageUrl = "/images/categories/chinese.png" },
                    new() { Name = "South Indian", ImageUrl = "/images/categories/south-indian.png" },
                    new() { Name = "Desserts", ImageUrl = "/images/categories/desserts.png" },
                    new() { Name = "Beverages", ImageUrl = "/images/categories/beverages.png" }
                };
                db.Categories.AddRange(categories);
                await db.SaveChangesAsync();
            }

            // Seed restaurants
            if (!db.Restaurants.Any())
            {
                var restaurants = new List<Restaurant>
                {
                    new() {
                        Name = "Pizza Hub",
                        Description = "Authentic Italian pizzas with fresh ingredients",
                        LogoUrl = "/images/restaurants/pizza-hub-logo.png",
                        BannerUrl = "/images/restaurants/pizza-hub-banner.jpg",
                        CuisineTags = "Pizza, Italian",
                        City = "Bangalore",
                        Address = "123, MG Road, Bangalore - 560001",
                        AvgRating = 4.5m,
                        RatingCount = 1200,
                        DeliveryTimeMinMinutes = 30,
                        DeliveryTimeMaxMinutes = 40,
                        FreeDeliveryAboveAmount = 199
                    },
                    new() {
                        Name = "Burger House",
                        Description = "Juicy burgers made fresh to order",
                        LogoUrl = "/images/restaurants/burger-house-logo.png",
                        BannerUrl = "/images/restaurants/burger-house-banner.jpg",
                        CuisineTags = "Burger, American",
                        City = "Bangalore",
                        Address = "45, Indiranagar, Bangalore - 560038",
                        AvgRating = 4.2m,
                        RatingCount = 860,
                        DeliveryTimeMinMinutes = 25,
                        DeliveryTimeMaxMinutes = 35,
                        FreeDeliveryAboveAmount = 149
                    },
                    new() {
                        Name = "Chinese Wok",
                        Description = "Authentic Chinese cuisine from the heart of Asia",
                        LogoUrl = "/images/restaurants/chinese-wok-logo.png",
                        BannerUrl = "/images/restaurants/chinese-wok-banner.jpg",
                        CuisineTags = "Chinese, Asian",
                        City = "Bangalore",
                        Address = "78, Koramangala, Bangalore - 560034",
                        AvgRating = 4.0m,
                        RatingCount = 650,
                        DeliveryTimeMinMinutes = 35,
                        DeliveryTimeMaxMinutes = 50,
                        FreeDeliveryAboveAmount = 299
                    },
                    new() {
                        Name = "Fast Place",
                        Description = "Quick bites and classic Indian fast food",
                        LogoUrl = "/images/restaurants/fast-place-logo.png",
                        BannerUrl = "/images/restaurants/fast-place-banner.jpg",
                        CuisineTags = "South Indian, Snacks",
                        City = "Bangalore",
                        Address = "12, Jayanagar, Bangalore - 560041",
                        AvgRating = 3.8m,
                        RatingCount = 430,
                        DeliveryTimeMinMinutes = 20,
                        DeliveryTimeMaxMinutes = 30,
                        FreeDeliveryAboveAmount = 99
                    }
                };
                db.Restaurants.AddRange(restaurants);
                await db.SaveChangesAsync();
            }

            // Seed food items
            if (!db.FoodItems.Any())
            {
                var pizzaHub = db.Restaurants.First(r => r.Name == "Pizza Hub");
                var burgerHouse = db.Restaurants.First(r => r.Name == "Burger House");
                var chineseWok = db.Restaurants.First(r => r.Name == "Chinese Wok");
                var fastPlace = db.Restaurants.First(r => r.Name == "Fast Place");

                var pizza = db.Categories.First(c => c.Name == "Pizza");
                var burger = db.Categories.First(c => c.Name == "Burger");
                var chinese = db.Categories.First(c => c.Name == "Chinese");
                var southIndian = db.Categories.First(c => c.Name == "South Indian");
                var desserts = db.Categories.First(c => c.Name == "Desserts");
                var beverages = db.Categories.First(c => c.Name == "Beverages");

                var foods = new List<FoodItem>
                {
                    // Pizza Hub
                    new() { RestaurantId = pizzaHub.RestaurantId, CategoryId = pizza.CategoryId, Name = "Margherita Pizza", Description = "Classic delight with 100% real mozzarella cheese", Price = 249, ImageUrl = "/images/foods/margherita.jpg", IsVeg = true },
                    new() { RestaurantId = pizzaHub.RestaurantId, CategoryId = pizza.CategoryId, Name = "Farmhouse Pizza", Description = "Loaded with fresh veggies", Price = 319, ImageUrl = "/images/foods/farmhouse.jpg", IsVeg = true },
                    new() { RestaurantId = pizzaHub.RestaurantId, CategoryId = pizza.CategoryId, Name = "Peppy Paneer Pizza", Description = "Topped with spicy paneer, capsicum, onion & tomatoes", Price = 349, ImageUrl = "/images/foods/peppy-paneer.jpg", IsVeg = true },
                    new() { RestaurantId = pizzaHub.RestaurantId, CategoryId = pizza.CategoryId, Name = "Cheese Burst Pizza", Description = "Extra cheese for cheese lovers", Price = 349, ImageUrl = "/images/foods/cheese-burst.jpg", IsVeg = true },
                    new() { RestaurantId = pizzaHub.RestaurantId, CategoryId = beverages.CategoryId, Name = "Coke (500ml)", Description = "Chilled refreshing cola", Price = 49, ImageUrl = "/images/foods/coke.jpg", IsVeg = true },

                    // Burger House
                    new() { RestaurantId = burgerHouse.RestaurantId, CategoryId = burger.CategoryId, Name = "Veg Burger", Description = "Crispy veggie patty with fresh toppings", Price = 179, ImageUrl = "/images/foods/veg-burger.jpg", IsVeg = true },
                    new() { RestaurantId = burgerHouse.RestaurantId, CategoryId = burger.CategoryId, Name = "Chicken Burger", Description = "Juicy grilled chicken patty", Price = 219, ImageUrl = "/images/foods/chicken-burger.jpg", IsVeg = false },
                    new() { RestaurantId = burgerHouse.RestaurantId, CategoryId = burger.CategoryId, Name = "Double Patty Burger", Description = "For the truly hungry — double the goodness", Price = 299, ImageUrl = "/images/foods/double-patty.jpg", IsVeg = false },
                    new() { RestaurantId = burgerHouse.RestaurantId, CategoryId = desserts.CategoryId, Name = "Chocolate Cake", Description = "Rich, moist chocolate layer cake", Price = 189, ImageUrl = "/images/foods/choc-cake.jpg", IsVeg = true },

                    // Chinese Wok
                    new() { RestaurantId = chineseWok.RestaurantId, CategoryId = chinese.CategoryId, Name = "Veg Fried Rice", Description = "Wok-tossed basmati rice with veggies", Price = 199, ImageUrl = "/images/foods/fried-rice.jpg", IsVeg = true },
                    new() { RestaurantId = chineseWok.RestaurantId, CategoryId = chinese.CategoryId, Name = "Chicken Manchurian", Description = "Crispy chicken in tangy manchurian sauce", Price = 249, ImageUrl = "/images/foods/manchurian.jpg", IsVeg = false },
                    new() { RestaurantId = chineseWok.RestaurantId, CategoryId = chinese.CategoryId, Name = "Hakka Noodles", Description = "Classic wok noodles with soy glaze", Price = 179, ImageUrl = "/images/foods/hakka.jpg", IsVeg = true },

                    // Fast Place
                    new() { RestaurantId = fastPlace.RestaurantId, CategoryId = southIndian.CategoryId, Name = "Masala Dosa", Description = "Crispy dosa with spiced potato filling", Price = 99, ImageUrl = "/images/foods/masala-dosa.jpg", IsVeg = true },
                    new() { RestaurantId = fastPlace.RestaurantId, CategoryId = southIndian.CategoryId, Name = "Idli Sambar", Description = "Soft steamed idlis with sambar & chutneys", Price = 79, ImageUrl = "/images/foods/idli.jpg", IsVeg = true },
                    new() { RestaurantId = fastPlace.RestaurantId, CategoryId = beverages.CategoryId, Name = "Filter Coffee", Description = "Authentic South Indian filter coffee", Price = 49, ImageUrl = "/images/foods/coffee.jpg", IsVeg = true }
                };
                db.FoodItems.AddRange(foods);
                await db.SaveChangesAsync();
            }

            // Seed coupons
            if (!db.Coupons.Any())
            {
                var coupons = new List<Coupon>
                {
                    new() { Code = "FIRST50", DiscountType = "Percentage", DiscountValue = 50, MinOrderValue = 200, ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true },
                    new() { Code = "FLAT100", DiscountType = "Flat", DiscountValue = 100, MinOrderValue = 400, ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true },
                    new() { Code = "SAVE20", DiscountType = "Percentage", DiscountValue = 20, MinOrderValue = 150, ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true }
                };
                db.Coupons.AddRange(coupons);
                await db.SaveChangesAsync();
            }
        }
    }
}
