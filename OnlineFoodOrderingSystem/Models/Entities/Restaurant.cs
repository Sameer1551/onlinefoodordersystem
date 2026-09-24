namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? CuisineTags { get; set; } // comma-separated: "Pizza, Italian"
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal AvgRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public int DeliveryTimeMinMinutes { get; set; } = 30;
        public int DeliveryTimeMaxMinutes { get; set; } = 40;
        public decimal FreeDeliveryAboveAmount { get; set; } = 199;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
