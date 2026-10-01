using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Endpoints;

/// <summary>
/// Minimal API-эндпоинты OrderService.
/// </summary>
public static class OrderEndpoints
{
    /// <summary>Зарегистрировать все эндпоинты заказов в приложении.</summary>
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // ─── POST /api/orders — создание заказа ─────────────────────────
        group.MapPost("/", async (
            CreateOrderDto dto,
            OrderDbContext db,
            ICatalogClient catalog,
            IPublishEndpoint bus,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            if (dto.Items is null || dto.Items.Count == 0)
                return Results.BadRequest(new { message = "Заказ не может быть пустым" });

            var order = new Order
            {
                UserId = dto.UserId,
                UserEmail = dto.UserEmail
            };

            foreach (var item in dto.Items)
            {
                var availability = await catalog.CheckAvailabilityAsync(
                    item.ProductId, item.Quantity, ct);

                if (availability is null)
                    return Results.BadRequest(new
                    {
                        message = $"Товар {item.ProductId} не найден или неактивен"
                    });

                if (!availability.InStock)
                    return Results.BadRequest(new
                    {
                        message = $"Недостаточно товара «{availability.ProductName}». Запрошено: {item.Quantity}"
                    });

                order.Items.Add(new OrderItem
                {
                    ProductId = availability.ProductId,
                    ProductName = availability.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = availability.CurrentPrice
                });
            }

            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            db.Orders.Add(order);
            await db.SaveChangesAsync(ct);

            await bus.Publish(new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                UserEmail = order.UserEmail,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice
                }).ToList()
            }, ct);

            logger.LogInformation(
                "Заказ {OrderId} создан на сумму {Total} и опубликован в RabbitMQ",
                order.Id, order.TotalAmount);

            return Results.Created($"/api/orders/{order.Id}", new
            {
                order.Id,
                order.TotalAmount,
                order.Status
            });
        })
        .WithSummary("Создать заказ (HTTP-вызов CatalogService + публикация OrderCreatedEvent)");

        // ─── GET /api/orders/{id} ──────────────────────────────────────
        group.MapGet("/{id:guid}", async (
            Guid id, OrderDbContext db, CancellationToken ct) =>
        {
            var order = await db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            return order is null
                ? Results.NotFound(new { message = $"Заказ {id} не найден" })
                : Results.Ok(order);
        })
        .WithSummary("Получить заказ по Id");

        // ─── GET /api/orders ───────────────────────────────────────────
        group.MapGet("/", async (OrderDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct)))
        .WithSummary("Список всех заказов");
    }
}