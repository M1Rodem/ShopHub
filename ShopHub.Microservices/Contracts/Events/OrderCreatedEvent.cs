namespace Contracts.Events;

/// <summary>
/// Событие, публикуемое OrderService при успешном создании заказа.
/// Подписчиком выступает NotificationService (и любые другие сервисы в будущем).
///
/// ВАЖНО: контракт события — это «договор» между сервисами.
/// Поля нельзя переименовывать/удалять без версионирования.
/// </summary>
public record OrderCreatedEvent
{
    /// <summary>Уникальный идентификатор созданного заказа (Guid, генерируется OrderService).</summary>
    public Guid OrderId { get; init; }

    /// <summary>Идентификатор пользователя, оформившего заказ.</summary>
    public Guid UserId { get; init; }

    /// <summary>Email покупателя — на него уйдёт письмо.</summary>
    public string UserEmail { get; init; } = string.Empty;

    /// <summary>Итоговая сумма заказа в рублях.</summary>
    public decimal TotalAmount { get; init; }

    /// <summary>Позиции заказа со снимками названия и цены на момент покупки.</summary>
    public List<OrderItemEvent> Items { get; init; } = [];

    /// <summary>Момент создания заказа (UTC).</summary>
    public DateTime CreatedAt { get; init; }
}