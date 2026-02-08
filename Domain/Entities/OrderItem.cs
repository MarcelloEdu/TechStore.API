namespace TechStore.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }
        public Order Order { get; private set; } = null!;

        public int ProductId { get; private set; }
        public Product Product { get; private set; } = null!;

        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Subtotal { get; private set; }

        private OrderItem() { }

        public OrderItem(int productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");

            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = quantity * unitPrice;
        }
    }
}

