using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data;

/// <summary>
/// DbContext для CatalogService.
/// Владеет только своей БД — SQLite-файлом catalog.db.
/// Никакие сущности других сервисов здесь не хранятся.
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    /// <summary>Таблица товаров.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Таблица категорий.</summary>
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.IsActive);

            // Seed-данные: два товара появятся сразу после первой миграции.
            // Guid'ы фиксированные — на них ссылаются примеры в requests.http.
            entity.HasData(
                new Product
                {
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    Name = "iPhone 15 Pro",
                    Description = "Смартфон Apple с чипом A17 Pro",
                    Price = 89990.00m,
                    Stock = 15,
                    Category = "Смартфоны",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = Guid.Parse("4ba96f75-6828-5673-c4ad-3d074a77bab7"),
                    Name = "MacBook Air M3",
                    Description = "Ноутбук Apple с чипом M3",
                    Price = 129990.00m,
                    Stock = 8,
                    Category = "Ноутбуки",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(c => c.Slug).IsUnique();
        });
    }
}