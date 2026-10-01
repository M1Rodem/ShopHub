using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

/// <summary>
/// DbContext OrderService. Владеет только своей БД — SQLite-файлом orders.db.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    /// <summary>Таблица заказов.</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Таблица позиций заказов.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserEmail).IsRequired().HasMaxLength(200);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Status).HasMaxLength(50);
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.CreatedAt);

            entity.HasMany(o => o.Items)
                  .WithOne()
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}