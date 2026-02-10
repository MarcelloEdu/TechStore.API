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

    // === Vendas por Produto ===
    [HttpGet("sales-by-product")]
    public async Task<IActionResult> GetSalesByProduct(
            DateTime? startDate,
            DateTime? endDate)
    {
        
        var query = _context.OrderItems
            .Include(i => i.Product) 
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(i => i.Order.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(i => i.Order.CreatedAt <= endDate.Value);
        
        var report = await query
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
    
    [HttpGet("sales-by-category")]
    public async Task<IActionResult> GetSalesByCategory(
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _context.OrderItems
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(i => i.Order.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(i => i.Order.CreatedAt <= endDate.Value);
        
        var report = await query
            .GroupBy(i => new
            {
                i.Product.CategoryId,
                i.Product.Category.Name
            })
            .Select(g => new SalesByCategoryResponse
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Subtotal)
            })
            .AsNoTracking()
            .ToListAsync();
            
        return Ok(report);
    }
    
    [HttpGet("sales-by-period")]
    public async Task<IActionResult> GetSalesByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
    {
        
        if (startDate > endDate)
            return BadRequest("Data inicial não pode ser maior que a final.");
        
        
        var data = await _context.OrderItems
            .Include(i => i.Order)  
            .Where(i =>
                i.Order.CreatedAt >= startDate &&
                i.Order.CreatedAt <= endDate
            )
            .GroupBy(_ => 1) 
            .Select(g => new SalesByPeriodResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Subtotal)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        
        
        return Ok(data ?? new SalesByPeriodResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalQuantitySold = 0,
            TotalRevenue = 0
        });
    }
}
