using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHub.Models;

public class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Text { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}