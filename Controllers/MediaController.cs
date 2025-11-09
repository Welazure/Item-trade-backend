using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Trading.Context;
using Trading.Models;

namespace Trading.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly TradeContext _context;
    private readonly IWebHostEnvironment _environment;

    public MediaController(TradeContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // POST: api/media/upload/{itemId}
    [HttpPost("upload/{itemId}")]
    public async Task<IActionResult> UploadMedia(Guid itemId, IFormFile file, [FromForm] bool isPrimary = false)
    {
        var item = await _context.Items.FindAsync(itemId);
        if (item == null) return NotFound("Item not found.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (item.UserId.ToString() != currentUserId && !isAdmin)
        {
            return Forbid("You can only upload media to your own items.");
        }

        if (file == null || file.Length == 0) return BadRequest("File is empty.");

        // Basic validation
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".avi" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Invalid file type.");
        }

        var mediaType = extension is ".mp4" or ".mov" or ".avi" ? "Video" : "Image";
        
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", itemId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // If setting this as primary, unset other primary media for the item
        if (isPrimary)
        {
            var existingPrimary = await _context.Media.FirstOrDefaultAsync(m => m.ItemId == itemId && m.IsPrimary);
            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
            }
        }

        // Check if there is any other media for this item. If not, this one becomes primary.
        var hasOtherMedia = await _context.Media.AnyAsync(m => m.ItemId == itemId);
        var isPrimaryMedia = isPrimary || !hasOtherMedia;

        var media = new Media
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            FileName = file.FileName,
            FilePath = $"/uploads/{itemId}/{uniqueFileName}", // Relative path for web access
            ContentType = file.ContentType,
            FileSize = file.Length,
            MediaType = mediaType,
            IsPrimary = isPrimaryMedia,
            UploadedAt = DateTime.UtcNow
        };

        _context.Media.Add(media);
        await _context.SaveChangesAsync();

        return Ok(media);
    }

    // DELETE: api/media/{mediaId}
    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> DeleteMedia(Guid mediaId)
    {
        var media = await _context.Media.Include(m => m.Item).FirstOrDefaultAsync(m => m.Id == mediaId);
        if (media == null) return NotFound("Media not found.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (media.Item.UserId.ToString() != currentUserId && !isAdmin)
        {
            return Forbid("You can only delete media from your own items.");
        }

        // Delete file from disk
        var fullPath = Path.Combine(_environment.WebRootPath, media.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.Media.Remove(media);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
