using System.Net;
using System.Net.Http.Json;

namespace OrderService.Services;

/// <summary>
/// HTTP-клиент CatalogService. Retry/CircuitBreaker настраиваются в Program.cs
/// через Polly при регистрации HttpClient.
/// </summary>
public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CatalogClient> _logger;

    public CatalogClient(HttpClient http, ILogger<CatalogClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ProductAvailabilityResponse?> CheckAvailabilityAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        var url = $"/api/products/{productId}/availability?quantity={quantity}";

        try
        {
            _logger.LogDebug("→ CatalogService: GET {Url}", url);

            var resp = await _http.GetAsync(url, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<ProductAvailabilityResponse>(
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка вызова CatalogService для товара {ProductId}", productId);
            throw;
        }
    }
}