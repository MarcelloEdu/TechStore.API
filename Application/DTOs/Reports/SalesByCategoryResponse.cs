namespace TechStore.Application.DTOs.Reports;

public class SalesByCategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
