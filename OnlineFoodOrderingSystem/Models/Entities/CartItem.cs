namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FoodId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser? User { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}
