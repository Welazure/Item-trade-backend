using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Trading.Dto;
using Xunit;

namespace Trading.Tests;

public class ProfileControllerTests : IClassFixture<TradingApiFactory>
{
    private readonly HttpClient _client;

    public ProfileControllerTests(TradingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenForNewUserAsync(string username)
    {
        var userDto = new UserRegister { Username = username, Password = "Password123!", Email = $"{username}@example.com", Name = username, Address = "123 Test St", PhoneNumber = username };
        await _client.PostAsJsonAsync("/api/auth/register", userDto);
        
        var loginDto = new UserLogin { Username = username, Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse.Token;
    }

    [Fact]
    public async Task GetMyProfile_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenForNewUserAsync("profile_user_1");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/profile/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("profile_user_1", profile.Username);
    }

    [Fact]
    public async Task GetMyProfile_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/profile/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenForNewUserAsync("update_user_1");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new UpdateProfileRequest
        {
            Email = "newemail@example.com",
            Name = "New Name",
            Address = "New Address",
            PhoneNumber = "0987654321"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/me", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateMyProfile_WithConflictingEmail_ReturnsConflict()
    {
        // Arrange
        await GetAuthTokenForNewUserAsync("existing_user"); // This user takes the email "existing_user@example.com"
        var token2 = await GetAuthTokenForNewUserAsync("conflicting_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        
        var updateDto = new UpdateProfileRequest
        {
            Email = "existing_user@example.com", // Try to take the email from the first user
            Name = "n", Address = "a", PhoneNumber = "p"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/me", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
