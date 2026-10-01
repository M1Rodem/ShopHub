using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Models;

namespace CatalogService.Services;

/// <summary>
/// Сервис управления товарами каталога.
/// Содержит бизнес-логику: CRUD, фильтрация, пагинация и кэширование в Redis.
/// </summary>
public class ProductService
{
    private readonly CatalogDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProductService> _logger;

    /// <summary>Время жизни записи товара в Redis.</summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public ProductService(
        CatalogDbContext db,
        IDistributedCache cache,
        ILogger<ProductService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Получить список активных товаров с фильтрацией и пагинацией.
    /// Список не кэшируется — он меняется часто.
    /// </summary>
    public async Task<(List<ProductDto> Items, int TotalCount)> GetAllAsync(
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.Price,
                p.Stock, p.Category, p.ImageUrl, p.CreatedAt))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <summary>
    /// Получить товар по Id. Сначала ищет в Redis, при промахе — в SQLite,
    /// затем кладёт результат в Redis на 5 минут.
    /// </summary>
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"product:{id}";

        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (cached is not null)
            {
                _logger.LogDebug("Товар {ProductId} получен из Redis", id);
                return JsonSerializer.Deserialize<ProductDto>(cached);
            }
        }
        catch (Exception ex)
        {
            // Redis недоступен — не критично, идём в БД.
            _logger.LogWarning(ex, "Ошибка чтения из Redis, читаем из БД");
        }

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

        if (product is null) return null;

        var dto = ToDto(product);

        try
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(dto),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheTtl
                },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось сохранить товар в Redis");
        }

        return dto;
    }

    /// <summary>Создать новый товар.</summary>
    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category.Trim(),
            ImageUrl = dto.ImageUrl
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Создан товар {ProductId}: {Name}", product.Id, product.Name);
        return ToDto(product);
    }

    /// <summary>Обновить товар. Возвращает null, если товар не найден или неактивен.</summary>
    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);
        if (product is null) return null;

        if (dto.Name is not null) product.Name = dto.Name.Trim();
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.Category is not null) product.Category = dto.Category.Trim();
        if (dto.ImageUrl is not null) product.ImageUrl = dto.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await SafeRemoveCacheAsync(id, ct);

        return ToDto(product);
    }

    /// <summary>Мягкое удаление: IsActive = false. Возвращает false, если товар не найден.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await SafeRemoveCacheAsync(id, ct);

        _logger.LogInformation("Товар {ProductId} помечен как удалённый", id);
        return true;
    }

    /// <summary>
    /// Проверить наличие товара и вернуть актуальную цену.
    /// Используется OrderService при создании заказа.
    /// Возвращает null, если товар не найден.
    /// </summary>
    public async Task<ProductAvailabilityDto?> CheckAvailabilityAsync(
        Guid id, int quantity, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

        if (product is null) return null;

        return new ProductAvailabilityDto(
            ProductId: product.Id,
            RequestedQuantity: quantity,
            InStock: product.Stock >= quantity,
            CurrentPrice: product.Price,
            ProductName: product.Name);
    }

    /// <summary>Аккуратно удалить ключ из Redis, проглатывая ошибки подключения.</summary>
    private async Task SafeRemoveCacheAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await _cache.RemoveAsync($"product:{id}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось удалить товар из Redis");
        }
    }

    /// <summary>Маппинг сущности Product в DTO.</summary>
    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Description, p.Price,
        p.Stock, p.Category, p.ImageUrl, p.CreatedAt);
}