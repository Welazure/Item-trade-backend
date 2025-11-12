using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Trading.Context;
using Trading.Dto;
using Trading.Models;

namespace Trading.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly TradeContext _context;

    public ItemController(TradeContext context)
    {
        _context = context;
    }

    // GET: api/item - Get approved items with optional filters
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApprovedItems([FromQuery] Guid? categoryId, [FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.User)
            .Include(i => i.Media)
            .Where(i => i.IsApproved && !i.Bookings.Any(b => b.IsActive));

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Name.Contains(searchTerm) || i.Description.Contains(searchTerm) || i.Request.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        
        var response = new PaginatedResponse<Item>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }

    // GET: api/item/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.User)
            .Include(i => i.Media)
            .Include(i => i.Bookings)
                .ThenInclude(b => b.BookerUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        if (!item.IsApproved)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Forbid("You must be logged in to view this item.");
            }
            var isAdmin = User.IsInRole("Admin");
            if (item.UserId.ToString() != currentUserId && !isAdmin)
            {
                return Forbid("You do not have permission to view this item.");
            }
        }

        return Ok(item);
    }

    // GET: api/item/my-items
    [HttpGet("my-items")]
    public async Task<IActionResult> GetUserItems()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var items = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Media)
            .Include(i => i.Bookings)
                .ThenInclude(b => b.BookerUser)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        return Ok(items);
    }

    // POST: api/item
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
        if (user == null) return NotFound("User not found.");
        var item = new Item
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Request = request.Request,
            CategoryId = request.CategoryId,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Items.Add(item);
        user.Points -= 1;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
    }

    // PUT: api/item/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (item.UserId.ToString() != currentUserId && !isAdmin)
        {
            return Forbid("You can only update your own items.");
        }

        // Update properties
        item.Name = request.Name;
        item.Description = request.Description;
        item.Request = request.Request;
        item.CategoryId = request.CategoryId;

        // If a non-admin user updates the item, it must be re-approved
        if (!isAdmin)
        {
            item.IsApproved = false;
        }

        await _context.SaveChangesAsync();
        return Ok(item);
    }


    // DELETE: api/item/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (item.UserId.ToString() != currentUserId && !isAdmin)
        {
            return Forbid("You can only delete your own items.");
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- Admin Routes ---

    // GET: api/item/pending
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingItems()
    {
        var items = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.User)
            .Include(i => i.Media)
            .Where(i => !i.IsApproved)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
        return Ok(items);
    }

    // POST: api/item/{id}/approve
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveItem(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();

        item.IsApproved = true;
        await _context.SaveChangesAsync();
        return Ok(item);
    }
    
    // --- Category Route ---
    
    // GET: api/item/categories
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return Ok(categories);
    }
}
