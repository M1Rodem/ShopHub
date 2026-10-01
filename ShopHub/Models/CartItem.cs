using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;

    // Цена на момент добавления в корзину (может устареть)
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [NotMapped]
    public decimal Subtotal => Price * Quantity;
}