using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;
using System.Security.Claims;

namespace ShopHub.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly CartService _carts;

    public CartController(CartService carts) => _carts = carts;

    public async Task<IActionResult> Index()
    {
        var cart = await _carts.GetOrCreateCartAsync(GetUserId());
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        try
        {
            await _carts.AddItemAsync(GetUserId(), productId, quantity);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _carts.RemoveItemAsync(GetUserId(), productId);
        return RedirectToAction(nameof(Index));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}