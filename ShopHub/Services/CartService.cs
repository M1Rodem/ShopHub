using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class CartService
{
    private readonly ShopHubDbContext _db;
    private readonly ProductService _products;

    public CartService(ShopHubDbContext db, ProductService products)
    {
        _db = db;
        _products = products;
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    public async Task AddItemAsync(int userId, int productId, int quantity = 1)
    {
        var product = await _products.GetByIdAsync(productId)
            ?? throw new InvalidOperationException("Товар не найден");

        if (product.Stock < quantity)
            throw new InvalidOperationException("Недостаточно товара");

        var cart = await GetOrCreateCartAsync(userId);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return;

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return cart.Items.Sum(i => i.Price * i.Quantity);
    }
}