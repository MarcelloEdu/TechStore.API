namespace TechStore.Application.DTOs.Reports;

public class ProductSalesTimelineResponse
{
    public DateTime Date { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
