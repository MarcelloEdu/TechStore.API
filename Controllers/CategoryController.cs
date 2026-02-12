using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Application.DTOs.Category;
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
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category(
                dto.Name,
                dto.Description
            );

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAllCategories),
                new { id = category.Id },
                category
            );
        }

        // ===== Listar =====
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int? id
        )
        {
            if(id.HasValue)
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (category == null)
                    return NotFound();

                return Ok(category);
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
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
}