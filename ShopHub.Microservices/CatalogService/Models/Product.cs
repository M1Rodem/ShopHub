namespace CatalogService.Models;

/// <summary>
/// Товар в каталоге магазина.
/// Идентификатор — Guid, чтобы OrderService и NotificationService
/// могли ссылаться на товар без коллизий с ID других сервисов.
/// </summary>
public class Product
{
    /// <summary>Уникальный идентификатор товара.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название товара (3–200 символов).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Подробное описание (опционально).</summary>
    public string? Description { get; set; }

    /// <summary>Цена в рублях. Всегда &gt; 0.</summary>
    public decimal Price { get; set; }

    /// <summary>Количество на складе. Не может быть отрицательным.</summary>
    public int Stock { get; set; }

    /// <summary>Название категории (денормализовано для простоты).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>URL изображения (опционально).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Флаг активности. Удаление — мягкое (IsActive = false).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего обновления (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}