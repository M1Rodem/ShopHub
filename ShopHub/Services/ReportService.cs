using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class ReportService
{
    private readonly ShopHubDbContext _db;

    public ReportService(ShopHubDbContext db) => _db = db;

    public async Task<Report> BuildAsync(DateTime from, DateTime to)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to &&
                        o.Status != "Cancelled")
            .ToListAsync();

        var report = new Report
        {
            From = from,
            To = to,
            OrdersCount = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            ProductsSold = orders.Sum(o => o.Items.Sum(i => i.Quantity))
        };

        report.TopProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductRow
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.PriceAtPurchase * i.Quantity)
            })
            .OrderByDescending(r => r.Revenue)
            .Take(10)
            .ToList();

        return report;
    }
}