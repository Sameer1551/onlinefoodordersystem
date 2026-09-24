namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public int AddressId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
            // Pending | Preparing | Packed | OutForDelivery | Delivered | Cancelled
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public decimal DeliveryFee { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public string? CouponCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "COD";
            // COD | UPI | Card | NetBanking | Wallet

        // Navigation
        public ApplicationUser? User { get; set; }
        public Restaurant? Restaurant { get; set; }
        public Address? Address { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
    }
}
