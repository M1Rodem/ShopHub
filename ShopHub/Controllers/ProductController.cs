using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;

namespace ShopHub.Controllers;

public class ProductController : Controller
{
    private readonly ProductService _products;
    private readonly ReviewService _reviews;

    public ProductController(ProductService products, ReviewService reviews)
    {
        _products = products;
        _reviews = reviews;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var items = categoryId.HasValue
            ? await _products.GetByCategoryAsync(categoryId.Value)
            : await _products.GetAllAsync();

        ViewBag.Categories = await _products.GetCategoriesAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        ViewBag.Reviews = await _reviews.GetForProductAsync(id);
        ViewBag.AverageRating = await _reviews.GetAverageRatingAsync(id);
        return View(product);
    }
}