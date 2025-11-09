using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trading.Context;
using Trading.Dto;
using Trading.Models;

namespace Trading.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ILogger<CategoryController> logger, TradeContext context) : ControllerBase
{
    private readonly ILogger<CategoryController> _logger = logger;
    private readonly TradeContext _context = context;

    // GET: api/category
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Items.Where(i => i.IsApproved))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    // POST: api/category
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} created", category.Id);

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    // PUT: api/category/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                category.Name = request.Name;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} updated", id);

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            if (category.Items != null && category.Items.Any())
            {
                return BadRequest("Cannot delete category with associated items");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} deleted", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the category");
        }
    }
}
