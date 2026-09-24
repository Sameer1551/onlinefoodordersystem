namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FoodId { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}
