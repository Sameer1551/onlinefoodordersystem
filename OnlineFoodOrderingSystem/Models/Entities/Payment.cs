namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string Method { get; set; } = "COD";
            // COD | UPI | Card | NetBanking | Wallet
        public string Status { get; set; } = "Pending";
            // Pending | Success | Failed
        public string? TransactionRef { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Order? Order { get; set; }
    }
}
