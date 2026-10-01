using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;
using System.Security.Claims;

namespace ShopHub.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly OrderService _orders;
    private readonly CartService _carts;

    public OrderController(OrderService orders, CartService carts)
    {
        _orders = orders;
        _carts = carts;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orders.GetByUserAsync(GetUserId());
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpGet]
    public IActionResult Checkout() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(string shippingAddress)
    {
        try
        {
            var order = await _orders.CreateFromCartAsync(GetUserId(), shippingAddress);
            return RedirectToAction("Pay", "Payment", new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View();
        }
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}