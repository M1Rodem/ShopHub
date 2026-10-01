using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services;
using System.Security.Claims;

namespace ShopHub.Controllers;

public class AccountController : Controller
{
    private readonly UserService _users;

    public AccountController(UserService users) => _users = users;

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string email, string password, string firstName, string lastName, string? phone)
    {
        try
        {
            var user = await _users.RegisterAsync(email, password, firstName, lastName, phone);
            await SignInAsync(user.Id, user.Email, user.Role);
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View();
        }
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _users.AuthenticateAsync(email, password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View();
        }

        await SignInAsync(user.Id, user.Email, user.Role);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var id = GetUserId();
        if (id == null) return RedirectToAction("Login");

        var user = await _users.GetByIdAsync(id.Value);
        if (user == null) return RedirectToAction("Login");

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string firstName, string lastName,
        string? phone, string? address)
    {
        var id = GetUserId();
        if (id == null) return RedirectToAction("Login");

        await _users.UpdateProfileAsync(id.Value, firstName, lastName, phone, address);
        return RedirectToAction(nameof(Profile));
    }

    private async Task SignInAsync(int userId, string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    private int? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }
}