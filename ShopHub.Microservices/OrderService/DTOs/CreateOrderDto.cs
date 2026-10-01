using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

/// <summary>
/// DTO для создания заказа. Клиент присылает только ID товара и количество —
/// актуальную цену и название OrderService получает из CatalogService.
/// </summary>
public record CreateOrderDto(
    [Required] Guid UserId,
    [Required, EmailAddress] string UserEmail,
    [Required, MinLength(1)] List<CreateOrderItemDto> Items
);

/// <summary>Позиция в запросе на создание заказа.</summary>
public record CreateOrderItemDto(
    [Required] Guid ProductId,
    [Required, Range(1, 1000)] int Quantity
);