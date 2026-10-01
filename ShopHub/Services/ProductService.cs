using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class ProductService
{
    private readonly ShopHubDbContext _db;

    public ProductService(ShopHubDbContext db) => _db = db;

    public Task<List<Product>> GetAllAsync() =>
        _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public Task<Product?> GetByIdAsync(int id) =>
        _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<Product>> GetByCategoryAsync(int categoryId) =>
        _db.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return;
        p.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> HasStockAsync(int productId, int quantity)
    {
        var p = await _db.Products.FindAsync(productId);
        return p != null && p.Stock >= quantity;
    }

    public async Task DecreaseStockAsync(int productId, int quantity)
    {
        var p = await _db.Products.FindAsync(productId)
            ?? throw new InvalidOperationException("Товар не найден");

        if (p.Stock < quantity)
            throw new InvalidOperationException("Недостаточно товара на складе");

        p.Stock -= quantity;
        await _db.SaveChangesAsync();
    }

    public Task<List<Category>> GetCategoriesAsync() =>
        _db.Categories.OrderBy(c => c.Name).ToListAsync();
}