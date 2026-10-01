namespace Contracts.Events;

/// <summary>
/// Позиция заказа внутри события <see cref="OrderCreatedEvent"/>.
/// Название и цена — снимок на момент покупки (snapshot).
/// Даже если товар в каталоге изменится, событие останется консистентным.
/// </summary>
public record OrderItemEvent
{
    /// <summary>ID товара в CatalogService.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Название товара на момент оформления заказа.</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>Количество единиц товара.</summary>
    public int Quantity { get; init; }

    /// <summary>Цена за единицу на момент оформления заказа.</summary>
    public decimal Price { get; init; }
}