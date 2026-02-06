//regra de negócio para o produto
namespace TechStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }

        public bool IsActive { get; private set; }

        // Relacionamento
        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        protected Product() { } // EF

        public Product(
            string name,
            string? description,
            decimal price,
            int stockQuantity,
            int categoryId
        )
        {
            if (price < 0)
                throw new ArgumentException("Preço não pode ser negativo");

            if (stockQuantity < 0)
                throw new ArgumentException("Quantidade em estoque não pode ser negativa");

            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            CategoryId = categoryId;

            IsActive = stockQuantity > 0;
            CreatedAt = DateTime.UtcNow;
        }

        // ===== Regras de domínio =====

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;

            if (StockQuantity > 0)
                IsActive = true;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            if (StockQuantity < quantity)
                throw new InvalidOperationException("Estoque insuficiente");

            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;

            if (StockQuantity == 0)
                IsActive = false;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Preço não pode ser negativo");

            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (StockQuantity == 0)
                throw new InvalidOperationException("Não é possível ativar um produto com estoque zero");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

//sugestao de commit: Implementando a entidade Product com regras de negócio para gerenciamento de estoque e preço.