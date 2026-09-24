namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FoodId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser? User { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}
