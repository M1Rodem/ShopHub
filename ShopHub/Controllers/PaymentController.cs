using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;

namespace ShopHub.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly PaymentService _payments;
    private readonly OrderService _orders;

    public PaymentController(PaymentService payments, OrderService orders)
    {
        _payments = payments;
        _orders = orders;
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int orderId)
    {
        var order = await _orders.GetByIdAsync(orderId);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int orderId, string provider)
    {
        var order = await _orders.GetByIdAsync(orderId);
        if (order == null) return NotFound();

        await _payments.ProcessPaymentAsync(orderId, order.TotalAmount, provider);
        await _orders.UpdateStatusAsync(orderId, "Paid");

        return RedirectToAction("Details", "Order", new { id = orderId });
    }
}