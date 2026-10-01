using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // "Created" | "Paid" | "Shipped" | "Completed" | "Cancelled"
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Created";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    // Навигация
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}