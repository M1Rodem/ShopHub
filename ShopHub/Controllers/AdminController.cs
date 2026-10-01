using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Models;
using ShopHub.Services;

namespace ShopHub.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ProductService _products;
    private readonly UserService _users;

    public AdminController(ProductService products, UserService users)
    {
        _products = products;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ProductsCount = (await _products.GetAllAsync()).Count;
        ViewBag.UsersCount = (await _users.GetAllAsync()).Count;
        return View();
    }

    // ----- Products -----

    public async Task<IActionResult> Products()
    {
        var products = await _products.GetAllAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.Categories = await _products.GetCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        await _products.CreateAsync(product);
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = await _products.GetCategoriesAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Product product)
    {
        await _products.UpdateAsync(product);
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _products.DeactivateAsync(id);
        return RedirectToAction(nameof(Products));
    }

    // ----- Users -----

    public async Task<IActionResult> Users()
    {
        var users = await _users.GetAllAsync();
        return View(users);
    }
}