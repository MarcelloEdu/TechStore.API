namespace TechStore.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }
        public Order Order { get; private set; } = null!;

        public int ProductId { get; private set; }
        public Product Product { get; private set; } = null!;

        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public decimal Subtotal => UnitPrice * Quantity;

        protected OrderItem() { } // EF

        public OrderItem(
            int productId,
            decimal unitPrice,
            int quantity
        )
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");

            if (unitPrice < 0)
                throw new ArgumentException("Preço unitário não pode ser negativo.");

            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
