using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trading.Context;
using Trading.Dto;
using Trading.Models;
using Xunit;

namespace Trading.Tests;

public class BookingControllerTests : IClassFixture<TradingApiFactory>
{
    private readonly HttpClient _client;
    private readonly TradingApiFactory _factory;

    public BookingControllerTests(TradingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Helper to get a token for a specific user
    private async Task<string> GetAuthTokenAsync(string username, string password = "Password123!")
    {
        var loginDto = new UserLogin { Username = username, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse.Token;
    }

    // Helper to create a user and return their ID
    private async Task<User> CreateUserAsync(string username)
    {
        var userDto = new UserRegister { Username = username, Password = "Password123!", Email = $"{username}@example.com", Name = username, Address = "123 Test St", PhoneNumber = username };
        await _client.PostAsJsonAsync("/api/auth/register", userDto);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
        return await context.Users.FirstAsync(u => u.Username == username);
    }

    // Helper to create and approve an item, returning the item
    private async Task<Item> CreateAndApproveItem(User owner)
    {
        var item = new Item { Id = System.Guid.NewGuid(), Name = "Bookable Item", UserId = owner.Id, IsApproved = true, CategoryId = System.Guid.NewGuid(), Description = "d", Request = "r" };
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
            context.Items.Add(item);
            await context.SaveChangesAsync();
        }
        return item;
    }

    [Fact]
    public async Task CreateBooking_OnAvailableItem_ReturnsCreated()
    {
        // Arrange
        var owner = await CreateUserAsync("owner_book_1");
        var booker = await CreateUserAsync("booker_1");
        var item = await CreateAndApproveItem(owner);
        
        var bookerToken = await GetAuthTokenAsync("booker_1");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bookerToken);

        // Act
        var response = await _client.PostAsync($"/api/booking/{item.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_OnOwnItem_ReturnsBadRequest()
    {
        // Arrange
        var owner = await CreateUserAsync("owner_book_2");
        var item = await CreateAndApproveItem(owner);

        var ownerToken = await GetAuthTokenAsync("owner_book_2");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        // Act
        var response = await _client.PostAsync($"/api/booking/{item.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_AsBooker_ReturnsOk()
    {
        // Arrange
        var owner = await CreateUserAsync("owner_book_3");
        var booker = await CreateUserAsync("booker_3");
        var item = await CreateAndApproveItem(owner);
        
        var booking = new Booking { Id = System.Guid.NewGuid(), ItemId = item.Id, BookerUserId = booker.Id, IsActive = true };
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        var bookerToken = await GetAuthTokenAsync("booker_3");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bookerToken);

        // Act
        var cancelResponse = await _client.PutAsync($"/api/booking/{booking.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
    }
}
