namespace CatalogService.Models;

/// <summary>
/// Категория товара.
/// В текущей версии не связана внешним ключом с Product —
/// категория у товара хранится строкой. Оставлено для будущего расширения.
/// </summary>
public class Category
{
    /// <summary>Уникальный идентификатор категории.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название категории (например, «Смартфоны»).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-совместимый слаг (например, «smartfony»).</summary>
    public string Slug { get; set; } = string.Empty;
}