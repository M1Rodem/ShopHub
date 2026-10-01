using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class ReviewService
{
    private readonly ShopHubDbContext _db;

    public ReviewService(ShopHubDbContext db) => _db = db;

    public Task<List<Review>> GetForProductAsync(int productId) =>
        _db.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Review> AddAsync(int productId, int userId, int rating, string? text)
    {
        var already = await _db.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

        if (already)
            throw new InvalidOperationException("Вы уже оставляли отзыв на этот товар");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Text = text
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int reviewId)
    {
        var review = await _db.Reviews.FindAsync(reviewId);
        if (review == null) return;
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        var ratings = await _db.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }
}