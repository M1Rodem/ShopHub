using Contracts.Events;
using MassTransit;

namespace NotificationService.Consumers;

/// <summary>
/// Потребитель события OrderCreatedEvent из RabbitMQ.
/// В реальном проекте здесь отправлялся бы email/SMS пользователю.
/// В учебном — логируем факт получения события.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var e = context.Message;

        _logger.LogInformation(
            "📧 [OrderCreatedEvent] Заказ {OrderId} | {Email} | {Total} | позиций: {Count}",
            e.OrderId, e.UserEmail, e.TotalAmount, e.Items.Count);

        foreach (var item in e.Items)
        {
            _logger.LogInformation(
                "   • {Name} x {Qty} = {Sum}",
                item.ProductName, item.Quantity, item.Quantity * item.Price);
        }

        return Task.CompletedTask;
    }
}