namespace TechStore.Application.DTOs.Reports
{
    public class SalesByPeriodResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
