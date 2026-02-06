using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

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
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive);

            if (!categoryExists)
                return BadRequest("Categoria inválida ou inativa.");

            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity,
                request.CategoryId
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
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    Category = new CategoryResponse
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
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    Category = new CategoryResponse
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

    // ===== DTO =====
    public record CreateProductRequest(
        string Name,
        string? Description,
        decimal Price,
        int StockQuantity,
        int CategoryId
    );

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public CategoryResponse Category { get; set; } = null!;
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

}
