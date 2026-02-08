namespace TechStore.Application.DTOs.Orders;

public record CreateOrderRequest(
    List<CreateOrderItemRequest> Items
);
