using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class OrderService
{
    private readonly ShopHubDbContext _db;
    private readonly CartService _carts;
    private readonly ProductService _products;

    public OrderService(ShopHubDbContext db, CartService carts, ProductService products)
    {
        _db = db;
        _carts = carts;
        _products = products;
    }

    public async Task<Order> CreateFromCartAsync(int userId, string shippingAddress)
    {
        var cart = await _carts.GetOrCreateCartAsync(userId);
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Корзина пуста");

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = shippingAddress,
            Status = "Created"
        };

        foreach (var item in cart.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId)
                ?? throw new InvalidOperationException($"Товар {item.ProductId} не найден");

            // СНИМОК товара
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                PriceAtPurchase = product.Price,
                Quantity = item.Quantity
            });

            order.TotalAmount += product.Price * item.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _carts.ClearAsync(userId);
        return order;
    }

    public Task<Order?> GetByIdAsync(int id) =>
        _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

    public Task<List<Order>> GetByUserAsync(int userId) =>
        _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public Task<List<Order>> GetAllAsync() =>
        _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task UpdateStatusAsync(int orderId, string status)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Заказ не найден");

        order.Status = status;
        await _db.SaveChangesAsync();
    }
}