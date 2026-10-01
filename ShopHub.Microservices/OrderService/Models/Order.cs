namespace OrderService.Models;

/// <summary>
/// Заказ пользователя. Хранится в собственной БД OrderService (orders.db).
/// </summary>
public class Order
{
    /// <summary>Уникальный идентификатор заказа.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID пользователя, оформившего заказ.</summary>
    public Guid UserId { get; set; }

    /// <summary>Email пользователя — для уведомления.</summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>Итоговая сумма по всем позициям.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Статус: Pending / Paid / Shipped / Cancelled.</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Дата создания (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Позиции заказа.</summary>
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>
/// Позиция заказа. Название и цена — снапшот на момент покупки.
/// </summary>
public class OrderItem
{
    /// <summary>Уникальный идентификатор позиции.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID заказа (внешний ключ).</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID товара в CatalogService.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Название товара на момент заказа.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Количество единиц.</summary>
    public int Quantity { get; set; }

    /// <summary>Цена за единицу на момент заказа.</summary>
    public decimal UnitPrice { get; set; }
}