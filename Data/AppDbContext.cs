using Microsoft.EntityFrameworkCore;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Data;

/// <summary>
/// AppDbContext is the main DbContext for the Campus Food ordering system
/// Manages all database entities and their configurations
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.Property(u => u.AvatarPath).HasMaxLength(500);
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired().HasDefaultValue("Student");
            entity.Property(u => u.IsActive).HasDefaultValue(true);

            entity.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_User_Username");
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_User_Email");
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.Name).HasMaxLength(255).IsRequired();
            entity.Property(p => p.Price).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.SellingTime).HasMaxLength(255).IsRequired();
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.IsAvailable).HasDefaultValue(true);

            entity.HasIndex(p => p.Category).HasDatabaseName("IX_Product_Category");
            entity.HasIndex(p => p.IsAvailable).HasDatabaseName("IX_Product_IsAvailable");
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.StudentName).HasMaxLength(255);
            entity.Property(o => o.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(o => o.Notes).HasMaxLength(1000);

            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<Issue>()
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(o => o.Status).HasDatabaseName("IX_Order_Status");
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => new { oi.OrderId, oi.MenuItemId });
            entity.Property(oi => oi.ItemName).HasMaxLength(255).IsRequired();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.UnitPrice).IsRequired();
        });

        // Configure Issue entity
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(i => i.IssueId);
            entity.Property(i => i.OrderId).IsRequired();
            entity.Property(i => i.Details).HasMaxLength(1000).IsRequired();
            entity.Property(i => i.Status).HasMaxLength(50);
            entity.Property(i => i.ImagePath).HasMaxLength(500);

            // Relationship: Issue references Order (Foreign Key)
            entity.HasOne(i => i.Order)
                .WithMany()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: One Issue can have many Notifications
            entity.HasMany(i => i.Notifications)
                .WithOne(n => n.Issue)
                .HasForeignKey(n => n.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.IssueId).IsRequired();
            entity.Property(n => n.Message).HasMaxLength(1000).IsRequired();

            // Relationship: Notification references Issue (Foreign Key)
            entity.HasOne(n => n.Issue)
                .WithMany(i => i.Notifications)
                .HasForeignKey(n => n.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.AuditLogId);
            entity.Property(a => a.OrderId).IsRequired();
            entity.Property(a => a.StaffId).IsRequired();
            entity.Property(a => a.ActionType).HasMaxLength(255).IsRequired();
        });

        // Configure Promotion entity
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(p => p.PromotionId);
            entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.DiscountRate).IsRequired();
            entity.Property(p => p.ExpiryDate).IsRequired();
            entity.Property(p => p.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
            entity.Property(p => p.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(p => p.LastUpdatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            // Create indexes for filtering
            entity.HasIndex(p => p.Status).HasDatabaseName("IX_Promotion_Status");
            entity.HasIndex(p => p.ExpiryDate).HasDatabaseName("IX_Promotion_ExpiryDate");
        });
        modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("real");
    }
}
