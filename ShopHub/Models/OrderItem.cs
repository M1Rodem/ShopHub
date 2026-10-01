using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }

    // СНИМОК товара на момент покупки
    [Required]
    [StringLength(300)]
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAtPurchase { get; set; }

    public int Quantity { get; set; }

    [NotMapped]
    public decimal Subtotal => PriceAtPurchase * Quantity;
}