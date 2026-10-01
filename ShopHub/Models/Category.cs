using System.ComponentModel.DataAnnotations;

namespace ShopHub.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    // Родительская категория (для дерева категорий)
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    // Навигация
    public ICollection<Product> Products { get; set; } = new List<Product>();
}