namespace OnlineFoodOrderingSystem.Models.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; } // e.g. /Order/Track/12345
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
