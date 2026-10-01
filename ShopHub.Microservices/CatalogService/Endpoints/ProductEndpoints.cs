using Microsoft.AspNetCore.Mvc;
using CatalogService.DTOs;
using CatalogService.Services;

namespace CatalogService.Endpoints;

/// <summary>
/// Minimal API-эндпоинты CatalogService.
/// Все маршруты сгруппированы под /api/products.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>Зарегистрировать все эндпоинты каталога в приложении.</summary>
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        // ─── GET /api/products ──────────────────────────────────────────────
        group.MapGet("/", async (
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            ProductService service = default!,
            CancellationToken ct = default) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, total) = await service.GetAllAsync(
                category, minPrice, maxPrice, page, pageSize, ct);

            return Results.Ok(new
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        })
        .WithSummary("Получить список товаров с фильтрацией и пагинацией");

        // ─── GET /api/products/{id} ─────────────────────────────────────────
        group.MapGet("/{id:guid}", async (
            Guid id,
            ProductService service,
            CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is null
                ? Results.NotFound(new { message = $"Товар {id} не найден" })
                : Results.Ok(product);
        })
        .WithSummary("Получить товар по Id");

        // ─── GET /api/products/{id}/availability ────────────────────────────
        group.MapGet("/{id:guid}/availability", async (
            Guid id,
            [FromQuery] int quantity,
            ProductService service,
            CancellationToken ct) =>
        {
            if (quantity <= 0)
                return Results.BadRequest(new { message = "quantity должно быть > 0" });

            var result = await service.CheckAvailabilityAsync(id, quantity, ct);
            return result is null
                ? Results.NotFound(new { message = $"Товар {id} не найден" })
                : Results.Ok(result);
        })
        .WithSummary("Проверить наличие товара (используется OrderService)");

        // ─── POST /api/products ─────────────────────────────────────────────
        group.MapPost("/", async (
            CreateProductDto dto,
            ProductService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/products/{created.Id}", created);
        })
        .WithSummary("Создать товар");

        // ─── PUT /api/products/{id} ─────────────────────────────────────────
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductDto dto,
            ProductService service,
            CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, dto, ct);
            return updated is null
                ? Results.NotFound(new { message = $"Товар {id} не найден" })
                : Results.Ok(updated);
        })
        .WithSummary("Обновить товар (частично)");

        // ─── DELETE /api/products/{id} ──────────────────────────────────────
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ProductService service,
            CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Товар {id} не найден" });
        })
        .WithSummary("Мягко удалить товар (IsActive = false)");
    }
}