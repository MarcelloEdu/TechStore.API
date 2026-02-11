
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;
using TechStore.Application.DTOs.Product;

namespace TechStore.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // ===== Criar =====
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return BadRequest("Categoria inválida ou inativa.");

            var product = new Product(
                dto.Name,
                dto.Description,
                dto.Price,
                dto.StockQuantity,
                dto.CategoryId
            );

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.Id },
                product
            );
        }

        // ===== Listar Todos =====
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? orderBy
        )
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = orderBy switch
            {
                "PriceAsc" => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),

                _ => query.OrderByDescending(p =>
                    _context.OrderItems
                        .Where(oi => oi.ProductId == p.Id)
                        .Sum(oi => (int?)oi.Quantity) ?? 0
                )
        };

        var products = await query
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    Category = new CategorySummaryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    }
                })
                .ToListAsync();

            return Ok(products);
        }


        // ===== Listar Por ID =====
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    Category = new CategorySummaryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    }
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // ===== Verificar Estoque =====
        [HttpGet("{id:int}/stock")]
        public async Task<IActionResult> GetProductStock(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Ok(new
            {
                product.Id,
                product.Name,
                product.StockQuantity,
                product.IsActive
            });
        }

        // ===== Repor estoque =====
        [HttpPut("{id:int}/stock/increase")]
        public async Task<IActionResult> IncreaseStock(int id, IncreaseStockDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.IncreaseStock(dto.Quantity);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // ===== Ativar =====
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Activate();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ===== Desativar =====
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Deactivate();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ===== Excluir =====
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
