namespace TechStore.Application.DTOs.Reports;

public class SalesByProductResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
