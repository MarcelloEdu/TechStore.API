//Entidade Categoria
//representa a categoria de um produto (periferico, hardware, etc.)
namespace TechStore.Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Relacionamento 1:N
        public ICollection<Product> Products { get; private set; } = new List<Product>();

        protected Category() { } // necessário para o EF

        public Category(string name, string? description)
        {
            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}