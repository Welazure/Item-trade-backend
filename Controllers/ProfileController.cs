using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Trading.Context;
using Trading.Models;
using Trading.Dto;

namespace Trading.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly TradeContext _context;

        public ProfileController(TradeContext context)
        {
            _context = context;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.Items)
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var profile = new ProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = user.RegisteredAt,
                ItemsCount = user.Items.Count,
                ActiveBookingsCount = user.Bookings.Count(b => b.IsActive),
                Role = user.Role.ToString(),
                Points = user.Points
            };

            return Ok(profile);
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check for potential conflicts with other users
            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Email == request.Email))
            {
                return Conflict("Email is already in use by another account.");
            }
            if (await _context.Users.AnyAsync(u => u.Id != userId && u.PhoneNumber == request.PhoneNumber))
            {
                return Conflict("Phone number is already in use by another account.");
            }

            user.Email = request.Email;
            user.Name = request.Name;
            user.Address = request.Address;
            user.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }
    }
}
