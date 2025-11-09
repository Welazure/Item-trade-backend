using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trading.Context;
using Trading.Models;

namespace Trading.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly TradeContext _context;

        public BookingController(TradeContext context)
        {
            _context = context;
        }

        // POST: api/booking/{itemId}
        [HttpPost("{itemId}")]
        public async Task<IActionResult> CreateBooking(Guid itemId)
        {
            var bookerUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = await _context.Items
                .Include(i => i.Bookings)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null || !item.IsApproved)
            {
                return NotFound("Item not available for booking.");
            }

            if (item.UserId == bookerUserId)
            {
                return BadRequest("You cannot book your own item.");
            }

            if (item.Bookings.Any(b => b.IsActive))
            {
                return Conflict("Item is already booked.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                BookerUserId = bookerUserId,
                BookedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await GetBookingById(booking.Id);
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, result.Value);
        }

        // GET: api/booking/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingById(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Item)
                    .ThenInclude(i => i.User)
                .Include(b => b.Item)
                    .ThenInclude(i => i.Category)
                .Include(b => b.Item)
                    .ThenInclude(i => i.Media)
                .Include(b => b.BookerUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Allow access to the booker, the item owner, or an admin
            if (booking.BookerUserId.ToString() != currentUserId && booking.Item.UserId.ToString() != currentUserId && !isAdmin)
            {
                return Forbid("You do not have permission to view this booking.");
            }

            return Ok(booking);
        }

        // GET: api/booking/my-bookings
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookings = await _context.Bookings
                .Include(b => b.Item.User)
                .Include(b => b.Item.Category)
                .Include(b => b.Item.Media)
                .Where(b => b.BookerUserId == userId && b.IsActive)
                .OrderByDescending(b => b.BookedAt)
                .ToListAsync();

            return Ok(bookings);
        }
        
        // GET: api/booking/item/{itemId}
        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetActiveBookingForItem(Guid itemId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookerUser)
                .FirstOrDefaultAsync(b => b.ItemId == itemId && b.IsActive);

            if (booking == null)
            {
                return NotFound("No active booking found for this item.");
            }
            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var itemOwnerId = await _context.Items.Where(i => i.Id == itemId).Select(i => i.UserId).FirstOrDefaultAsync();
            var isAdmin = User.IsInRole("Admin");

            if (booking.BookerUserId.ToString() != currentUserId && itemOwnerId.ToString() != currentUserId && !isAdmin)
            {
                return Forbid("You do not have permission to view this booking.");
            }

            return Ok(booking);
        }

        // PUT: api/booking/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Item)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null || !booking.IsActive)
            {
                return NotFound("Active booking not found.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Only the booker or the item owner can cancel
            if (booking.BookerUserId.ToString() != currentUserId && booking.Item.UserId.ToString() != currentUserId)
            {
                return Forbid("You do not have permission to cancel this booking.");
            }

            booking.IsActive = false;
            booking.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Booking cancelled successfully.");
        }
    }
}
