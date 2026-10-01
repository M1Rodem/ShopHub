using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class SearchService
{
    private readonly ShopHubDbContext _db;

    public SearchService(ShopHubDbContext db) => _db = db;

    // Сейчас — простой SQL LIKE. В будущем — Elasticsearch.
    public Task<List<Product>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<Product>());

        var pattern = $"%{query.Trim()}%";

        return _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                (EF.Functions.Like(p.Name, pattern) ||
                 EF.Functions.Like(p.Description ?? "", pattern)))
            .Take(50)
            .ToListAsync();
    }
}