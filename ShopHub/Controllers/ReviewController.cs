using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;
using System.Security.Claims;

namespace ShopHub.Controllers;

public class ReviewController : Controller
{
    private readonly ReviewService _reviews;

    public ReviewController(ReviewService reviews) => _reviews = reviews;

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int rating, string? text)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _reviews.AddAsync(productId, userId, rating, text);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Product", new { id = productId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int productId)
    {
        await _reviews.DeleteAsync(id);
        return RedirectToAction("Details", "Product", new { id = productId });
    }
}