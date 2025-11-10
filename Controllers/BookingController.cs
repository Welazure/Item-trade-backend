using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trading.Context;
using Trading.Dto;
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
        public async Task<ActionResult<BookingDetailsDto>> GetBookingById(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Item)
                    .ThenInclude(i => i.User)
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

            var bookingDto = new BookingDetailsDto
            {
                Id = booking.Id,
                ItemId = booking.ItemId,
                ItemName = booking.Item.Name,
                BookedAt = booking.BookedAt,
                IsActive = booking.IsActive,
                Booker = new ProfileDto
                {
                    Id = booking.BookerUser.Id,
                    Username = booking.BookerUser.Username,
                    Email = booking.BookerUser.Email,
                    Name = booking.BookerUser.Name,
                    Address = booking.BookerUser.Address,
                    PhoneNumber = booking.BookerUser.PhoneNumber,
                    RegisteredAt = booking.BookerUser.RegisteredAt
                },
                ItemOwner = new UserContactDto
                {
                    Id = booking.Item.User.Id,
                    Name = booking.Item.User.Name,
                    PhoneNumber = booking.Item.User.PhoneNumber,
                    Email = booking.Item.User.Email
                }
            };

            return Ok(bookingDto);
        }

        // GET: api/booking/my-bookings
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookings = await _context.Bookings
                .Include(b => b.Item.User)
                .Include(b => b.BookerUser)
                .Where(b => b.BookerUserId == userId) // Show all bookings, active or not
                .OrderByDescending(b => b.BookedAt)
                .Select(booking => new BookingDetailsDto
                {
                    Id = booking.Id,
                    ItemId = booking.ItemId,
                    ItemName = booking.Item.Name,
                    BookedAt = booking.BookedAt,
                    IsActive = booking.IsActive,
                    ItemOwner = new UserContactDto
                    {
                        Id = booking.Item.User.Id,
                        Name = booking.Item.User.Name,
                        PhoneNumber = booking.Item.User.PhoneNumber,
                        Email = booking.Item.User.Email
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/booking/my-items-bookings
        [HttpGet("my-items-bookings")]
        public async Task<IActionResult> GetBookingsOnUserItems()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookings = await _context.Bookings
                .Include(b => b.Item)
                .Include(b => b.BookerUser)
                .Where(b => b.Item.UserId == userId) // Bookings on items owned by the current user
                .OrderByDescending(b => b.BookedAt)
                .Select(booking => new BookingDetailsDto
                {
                    Id = booking.Id,
                    ItemId = booking.ItemId,
                    ItemName = booking.Item.Name,
                    BookedAt = booking.BookedAt,
                    IsActive = booking.IsActive,
                    Booker = new ProfileDto
                    {
                        Id = booking.BookerUser.Id,
                        Username = booking.BookerUser.Username,
                        Email = booking.BookerUser.Email,
                        Name = booking.BookerUser.Name,
                        Address = booking.BookerUser.Address,
                        PhoneNumber = booking.BookerUser.PhoneNumber,
                        RegisteredAt = booking.BookerUser.RegisteredAt
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/booking/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Item.User) // Item Owner
                .Include(b => b.BookerUser) // Booker
                .OrderByDescending(b => b.BookedAt)
                .Select(booking => new BookingDetailsDto
                {
                    Id = booking.Id,
                    ItemId = booking.ItemId,
                    ItemName = booking.Item.Name,
                    BookedAt = booking.BookedAt,
                    IsActive = booking.IsActive,
                    Booker = new ProfileDto 
                    {
                        Id = booking.BookerUser.Id,
                        Username = booking.BookerUser.Username,
                        Name = booking.BookerUser.Name,
                        Email = booking.BookerUser.Email,
                        PhoneNumber = booking.BookerUser.PhoneNumber,
                        Address = booking.BookerUser.Address,
                        RegisteredAt = booking.BookerUser.RegisteredAt
                    },
                    ItemOwner = new UserContactDto
                    {
                        Id = booking.Item.User.Id,
                        Name = booking.Item.User.Name,
                        PhoneNumber = booking.Item.User.PhoneNumber,
                        Email = booking.Item.User.Email
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // PUT: api/booking/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Item)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound("Booking not found.");
            if (!booking.IsActive) return BadRequest("Booking is already cancelled.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Allow the booker, the item owner, or an admin to cancel
            if (booking.BookerUserId.ToString() != currentUserId && booking.Item.UserId.ToString() != currentUserId && !isAdmin)
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
