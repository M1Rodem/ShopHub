using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    // "Stripe" | "YooKassa" | ...
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = string.Empty;

    // "Pending" | "Completed" | "Failed" | "Refunded"
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    // ID транзакции у провайдера
    [StringLength(200)]
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}