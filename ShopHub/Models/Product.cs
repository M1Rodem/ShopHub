using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Продавец (может быть администратор)
    public int? SellerId { get; set; }
    public User? Seller { get; set; }

    // Изображения (храним как JSON-строку или через отдельную таблицу — здесь строка для простоты)
    [StringLength(2000)]
    public string? Images { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}