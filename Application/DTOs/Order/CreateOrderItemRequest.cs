namespace TechStore.Application.DTOs.Orders;

public record CreateOrderItemRequest(
    int ProductId,
    int Quantity
);
