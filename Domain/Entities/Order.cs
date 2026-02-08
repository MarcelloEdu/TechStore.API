using TechStore.Domain.Enums;

namespace TechStore.Domain.Entities;

public class Order
{
    public int Id { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    // EF
    private Order() { }

    public Order(IEnumerable<OrderItem> items)
    {
        if (!items.Any())
            throw new InvalidOperationException("Um pedido deve possuir ao menos um item.");

        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        foreach (var item in items)
        {
            AddItem(item);
        }

        CalculateTotal();
    }

    private void AddItem(OrderItem item)
    {
        Items.Add(item);
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Subtotal);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Apenas pedidos pendentes podem ser confirmados.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Confirmed)
            throw new InvalidOperationException("Pedidos confirmados não podem ser cancelados.");

        Status = OrderStatus.Cancelled;
    }
}
