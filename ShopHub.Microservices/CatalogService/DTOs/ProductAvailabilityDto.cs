namespace CatalogService.DTOs;

/// <summary>
/// Ответ эндпоинта /availability.
/// Используется OrderService для проверки остатка и получения актуальной цены.
/// </summary>
public record ProductAvailabilityDto(
    Guid ProductId,
    int RequestedQuantity,
    bool InStock,
    decimal CurrentPrice,
    string ProductName
);