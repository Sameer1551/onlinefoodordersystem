namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Address
    {
        public int AddressId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Label { get; set; } = "Home"; // Home | Work | Other
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
