using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
using TechStore.Application.DTOs.Reports;
using TechStore.Domain.Enums;

namespace TechStore.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("sales-by-product")]
    public async Task<IActionResult> GetSalesByProduct()
    {
        var report = await _context.OrderItems
            .Include(i => i.Product)
            .GroupBy(i => new
            {
                i.ProductId,
                i.Product.Name
            })
            .Select(g => new SalesByProductResponse
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Subtotal)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(report);
    }

    [HttpGet("product-sales-timeline/{productId:int}")]
    public async Task<IActionResult> GetProductSalesTimeline(int productId)
    {
        var report = await _context.OrderItems
            .Where(i =>
                i.ProductId == productId &&
                i.Order.Status == OrderStatus.Confirmed
            )
            .GroupBy(i => i.Order.CreatedAt.Date)
            .Select(g => new ProductSalesTimelineResponse
            {
                Date = g.Key,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Subtotal)
            })
            .OrderBy(x => x.Date)
            .AsNoTracking()
            .ToListAsync();

        return Ok(report);
    }


}
