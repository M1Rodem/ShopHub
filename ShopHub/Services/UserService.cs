using Microsoft.EntityFrameworkCore;
using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class UserService
{
    private readonly ShopHubDbContext _db;

    public UserService(ShopHubDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> RegisterAsync(string email, string password,
        string firstName, string lastName, string? phone = null)
    {
        if (await GetByEmailAsync(email) != null)
            throw new InvalidOperationException("Email уже занят");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Role = "Customer"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateProfileAsync(int userId, string firstName,
        string lastName, string? phone, string? address)
    {
        var user = await GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Пользователь не найден");

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Phone = phone;
        user.Address = address;
        await _db.SaveChangesAsync();
    }

    public Task<List<User>> GetAllAsync() =>
        _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
}