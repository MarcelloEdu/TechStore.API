
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;
using TechStore.Application.DTOs.Product;
using TechStore.Domain.Enums;   

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
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (category == null)
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

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                Category = new CategorySummaryDto
                {
                    Id = category.Id,
                    Name = category.Name
                }
            };

            return CreatedAtAction(
                nameof(GetProducts),
                new { id = product.Id },
                response
            );
        }


        // ===== Listar Produtos (com filtros opcionais) =====
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? id,
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] ProductOrderBy? orderBy
        )
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            // ===== Filtro por ID =====
            if (id.HasValue)
                query = query.Where(p => p.Id == id.Value);

            // ===== Filtros opcionais =====
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // ===== Ordenação =====
            query = orderBy switch
            {
                ProductOrderBy.PriceAsc => query.OrderBy(p => p.Price),

                ProductOrderBy.PriceDesc => query.OrderByDescending(p => p.Price),

                ProductOrderBy.NameAsc => query.OrderBy(p => p.Name),

                ProductOrderBy.NameDesc => query.OrderByDescending(p => p.Name),

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

            // Se foi passado ID, retorna objeto único
            if (id.HasValue)
                return products.Any() ? Ok(products.First()) : NotFound();

            return Ok(products);
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
