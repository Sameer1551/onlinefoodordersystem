namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Coupon
    {
        public int CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? RestaurantId { get; set; } // null = platform-wide
        public string DiscountType { get; set; } = "Flat"; // "Percentage" | "Flat"
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Restaurant? Restaurant { get; set; }
    }
}
