using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sodium;
using Trading.Context;
using Trading.Dto;
using Trading.Models;

namespace Trading.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TradeContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(TradeContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLogin.Username);

        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        try
        {
            if (!PasswordHash.ArgonHashStringVerify(user.Password, userLogin.Password))
            {
                return Unauthorized("Invalid credentials.");
            }
        }
        catch
        {
            // PasswordHash verification can throw an exception for invalid hashes
            return Unauthorized("Invalid credentials.");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegister userRegister)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if username, email, or phone number already exists in a single query
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => 
            u.Username == userRegister.Username || 
            u.Email == userRegister.Email || 
            u.PhoneNumber == userRegister.PhoneNumber);

        if (existingUser != null)
        {
            if (existingUser.Username == userRegister.Username)
            {
                return BadRequest("Username already exists.");
            }
            if (existingUser.Email == userRegister.Email)
            {
                return BadRequest("Email already exists.");
            }
            if (existingUser.PhoneNumber == userRegister.PhoneNumber)
            {
                return BadRequest("Phone number already exists.");
            }
        }

        var hashedPassword = PasswordHash.ArgonHashString(userRegister.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = userRegister.Username,
            Password = hashedPassword,
            Email = userRegister.Email,
            Name = userRegister.Name,
            Address = userRegister.Address,
            PhoneNumber = userRegister.PhoneNumber,
            Role = Role.User,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "your_super_secret_key_that_is_at_least_32_characters_long!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"] ?? "tradingwebsite.com",
            Audience = _configuration["Jwt:Audience"] ?? "tradingwebsite.com",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
