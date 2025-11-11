﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trading.Context;
using Trading.Dto;
using Trading.Models;

namespace Trading.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class PointsController : ControllerBase
{
    private readonly TradeContext _context;

    public PointsController(TradeContext context)
    {
        _context = context;
    }

    // Use constants to avoid magic strings
    private const string Package5000 = "package_5000";
    private const string Package10000 = "package_10000";

    // DTO for the buy points request
    public class BuyPointsRequest
    {
        public string PackageId { get; set; }
    }

    // GET: api/points/balance
    [HttpGet("balance")]
    public async Task<IActionResult> GetPointsBalance()
    {
        var (user, errorResult) = await GetCurrentUserAsync();
        if (errorResult != null)
        {
            return NotFound("User not found.");
        }

        return Ok(new { user.Points });
    }

    // GET: api/points/user
    [HttpGet("user")]
    public async Task<ActionResult<UserDto>> GetUserWithPoints()
    {
        var (user, errorResult) = await GetCurrentUserAsync();
        if (errorResult != null)
        {
            return NotFound("User not found.");
        }

        // Map the User entity to a UserDto
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Name = user.Name,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            Points = user.Points
        };

        return Ok(userDto);
    }

    // POST: api/points/buy
    [HttpPost("buy")]
    public async Task<IActionResult> BuyPoints([FromBody] BuyPointsRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.PackageId))
        {
            return BadRequest("Invalid package selection.");
        }

        var (user, errorResult) = await GetCurrentUserAsync();
        if (errorResult != null)
        {
            return NotFound("User not found.");
        }

        switch (request.PackageId)
        {
            case Package5000: // 3 points for Rp. 5000
                user.Points += 3;
                break;
            case Package10000: // 8 points for Rp. 10000
                user.Points += 8;
                break;
            default:
                return BadRequest("Invalid package ID.");
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Points added successfully.", NewBalance = user.Points });
    }
    
    /// <summary>
    /// Helper method to get the current authenticated user from the database.
    /// </summary>
    /// <returns>A tuple containing the User and a potential error ActionResult.</returns>
    private async Task<(User? user, ActionResult? errorResult)> GetCurrentUserAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
        {
            return (null, Unauthorized("Invalid user identifier."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return (null, NotFound("User not found."));
        }

        return (user, null);
    }
}