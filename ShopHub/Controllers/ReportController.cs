using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;

namespace ShopHub.Controllers;

[Authorize(Roles = "Admin")]
public class ReportController : Controller
{
    private readonly ReportService _reports;

    public ReportController(ReportService reports) => _reports = reports;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var f = from ?? DateTime.UtcNow.AddDays(-30);
        var t = to ?? DateTime.UtcNow;

        var report = await _reports.BuildAsync(f, t);
        return View(report);
    }
}