using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Trading.Dto;
using Xunit;

namespace Trading.Tests;

// Helper class to deserialize the auth response
public class AuthResponse
{
    public string Token { get; set; }
}

public class AuthControllerTests : IClassFixture<TradingApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TradingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var registerDto = new UserRegister
        {
            Username = "testuser_reg",
            Password = "Password123!",
            Email = "test_reg@example.com",
            Name = "Test User",
            Address = "123 Test St",
            PhoneNumber = "1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content?.Token);
        Assert.False(string.IsNullOrEmpty(content.Token));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserRegister { Username = "duplicate_user", Password = "p1", Email = "e1@e.com", Name="n", Address="a", PhoneNumber="p" };
        await _client.PostAsJsonAsync("/api/auth/register", userDto); // First registration

        var duplicateUserDto = new UserRegister { Username = "duplicate_user", Password = "p2", Email = "e2@e.com", Name="n2", Address="a2", PhoneNumber="p2" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateUserDto); // Second registration with same username

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var user = new UserRegister { Username = "loginuser_valid", Password = "Password123!", Email = "login_valid@e.com", Name="n", Address="a", PhoneNumber="p_valid" };
        await _client.PostAsJsonAsync("/api/auth/register", user);
        var loginDto = new UserLogin { Username = "loginuser_valid", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content?.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLogin { Username = "nonexistent_user", Password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
