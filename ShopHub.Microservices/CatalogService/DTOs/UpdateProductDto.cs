namespace CatalogService.DTOs;

/// <summary>
/// DTO для частичного обновления товара.
/// Все поля опциональны: null означает «не менять».
/// </summary>
public record UpdateProductDto(
    string? Name,
    string? Description,
    decimal? Price,
    int? Stock,
    string? Category,
    string? ImageUrl
);