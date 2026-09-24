using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineFoodOrderingSystem.Models.Entities;

namespace OnlineFoodOrderingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique constraint: one CartItem per user per food
            builder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.FoodId }).IsUnique();

            // Unique constraint: one Wishlist entry per user per food
            builder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.FoodId }).IsUnique();

            // Decimal precision for money fields
            builder.Entity<FoodItem>()
                .Property(f => f.Price).HasColumnType("decimal(10,2)");

            builder.Entity<Order>()
                .Property(o => o.SubTotal).HasColumnType("decimal(10,2)");
            builder.Entity<Order>()
                .Property(o => o.GST).HasColumnType("decimal(10,2)");
            builder.Entity<Order>()
                .Property(o => o.DeliveryFee).HasColumnType("decimal(10,2)");
            builder.Entity<Order>()
                .Property(o => o.Discount).HasColumnType("decimal(10,2)");
            builder.Entity<Order>()
                .Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");

            builder.Entity<Coupon>()
                .Property(c => c.DiscountValue).HasColumnType("decimal(10,2)");
            builder.Entity<Coupon>()
                .Property(c => c.MinOrderValue).HasColumnType("decimal(10,2)");

            builder.Entity<Restaurant>()
                .Property(r => r.AvgRating).HasColumnType("decimal(3,2)");
            builder.Entity<Restaurant>()
                .Property(r => r.FreeDeliveryAboveAmount).HasColumnType("decimal(10,2)");

            // Avoid cascade delete issues with multiple paths
            builder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
