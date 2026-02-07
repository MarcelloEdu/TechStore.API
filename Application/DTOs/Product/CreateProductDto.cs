namespace TechStore.Application.DTOs.Product;

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }
}
