using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace TechStore.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // ===== Criar =====
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var category = new Category(
                request.Name,
                request.Description
            );

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                category
            );
        }

        // ===== Listar Todos =====
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // ===== Listar Por ID =====
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // ===== Desativar =====
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            category.Deactivate();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ===== Ativar =====
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            category.Activate();
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ===== DTO =====
    public record CreateCategoryRequest(string Name, string? Description);
}
