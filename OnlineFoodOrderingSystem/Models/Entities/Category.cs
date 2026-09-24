namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    }
}
