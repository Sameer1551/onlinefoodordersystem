using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Models.ViewModels
{
    public class RestaurantListViewModel
    {
        public List<Restaurant> Restaurants { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? CuisineFilter { get; set; }
        public string? SortBy { get; set; } // "rating" | "delivery" | "name"
    }

    public class RestaurantMenuViewModel
    {
        public Restaurant Restaurant { get; set; } = null!;
        public List<Category> Categories { get; set; } = new();
        public List<FoodItem> FoodItems { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public bool? VegOnly { get; set; }
        public string? PriceRange { get; set; }
        public int CartItemCount { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public Restaurant? Restaurant { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public string? AppliedCouponCode { get; set; }
        public decimal Total => SubTotal + GST + DeliveryFee - Discount;
    }

    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = null!;
        public List<Address> Addresses { get; set; } = new();
        public int? SelectedAddressId { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string? CouponCode { get; set; }
        public string? CouponMessage { get; set; }
        public decimal Discount { get; set; }
    }

    public class OrderSummaryViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
        public string StatusLabel => Order.Status switch
        {
            "Pending" => "Order Placed",
            "Preparing" => "Preparing",
            "Packed" => "Packed",
            "OutForDelivery" => "Out for Delivery",
            "Delivered" => "Delivered",
            "Cancelled" => "Cancelled",
            _ => Order.Status
        };
        public int CurrentStepIndex => Order.Status switch
        {
            "Pending" => 0,
            "Preparing" => 1,
            "Packed" => 2,
            "OutForDelivery" => 3,
            "Delivered" => 4,
            _ => 0
        };
    }

    public class AdminDashboardViewModel
    {
        public int TotalRestaurants { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public List<DailySales> MonthlySales { get; set; } = new();
        public List<TopFoodItem> TopFoods { get; set; } = new();
    }

    public class DailySales
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class TopFoodItem
    {
        public string FoodName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public int TotalOrdered { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class AddressViewModel
    {
        public int AddressId { get; set; }
        public string Label { get; set; } = "Home";
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class AddToCartResult
    {
        public bool IsSuccess { get; set; }
        public bool IsConflict { get; set; }
        public int? ExistingRestaurantId { get; set; }
        public string? ExistingRestaurantName { get; set; }
        public int? NewRestaurantId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }

        public static AddToCartResult Success() => new() { IsSuccess = true };
        public static AddToCartResult RestaurantConflict(int existingId, string existingName, int newId, int foodId, int qty) =>
            new() { IsConflict = true, ExistingRestaurantId = existingId, ExistingRestaurantName = existingName, NewRestaurantId = newId, FoodId = foodId, Quantity = qty };
    }
}
