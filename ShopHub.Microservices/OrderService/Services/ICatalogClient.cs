namespace OrderService.Services;

/// <summary>
/// HTTP-клиент для синхронного взаимодействия с CatalogService.
/// </summary>
public interface ICatalogClient
{
    /// <summary>
    /// Проверить наличие товара и получить его актуальную цену и название.
    /// Возвращает null, если товар не найден или неактивен.
    /// </summary>
    Task<ProductAvailabilityResponse?> CheckAvailabilityAsync(
        Guid productId, int quantity, CancellationToken ct = default);
}

/// <summary>
/// Ответ CatalogService на GET /api/products/{id}/availability.
/// </summary>
public record ProductAvailabilityResponse(
    Guid ProductId,
    int RequestedQuantity,
    bool InStock,
    decimal CurrentPrice,
    string ProductName
);