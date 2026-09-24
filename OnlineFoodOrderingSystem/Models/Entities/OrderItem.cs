namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // price snapshot at time of order

        // Navigation
        public Order? Order { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}
