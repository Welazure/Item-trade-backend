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

public class ItemControllerTests : IClassFixture<TradingApiFactory>
{
    private readonly HttpClient _client;
    private readonly TradingApiFactory _factory;

    public ItemControllerTests(TradingApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
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

    // Helper to create a user and return their details
    private async Task<User> CreateUserAsync(string username)
    {
        var userDto = new UserRegister { Username = username, Password = "Password123!", Email = $"{username}@example.com", Name = username, Address = "123 Test St", PhoneNumber = username };
        var response = await _client.PostAsJsonAsync("/api/auth/register", userDto);
        response.EnsureSuccessStatusCode();
        // To get the actual user ID, we'd need another endpoint or to query the DB.
        // For now, we'll rely on other methods.
        // This is a simplification for the test.
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
        return await context.Users.FirstAsync(u => u.Username == username);
    }

    [Fact]
    public async Task CreateItem_WithValidDataAndAuth_ReturnsCreated()
    {
        // Arrange
        var user = await CreateUserAsync("item_creator");
        var token = await GetAuthTokenAsync("item_creator");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a category to reference
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
        var category = new Category { Id = System.Guid.NewGuid(), Name = "Test Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var createItemDto = new CreateItemRequest { Name = "New Item", Description = "Desc", Request = "Req", CategoryId = category.Id };

        // Act
        var response = await _client.PostAsJsonAsync("/api/item", createItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetItemById_ForUnapprovedItemByOwner_ReturnsOk()
    {
        // Arrange
        var owner = await CreateUserAsync("owner_user");
        var token = await GetAuthTokenAsync("owner_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var category = new Category { Id = System.Guid.NewGuid(), Name = "Category" };
        var item = new Item { Id = System.Guid.NewGuid(), Name = "Unapproved Item", UserId = owner.Id, IsApproved = false, CategoryId = category.Id, Description = "d", Request = "r" };

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
            context.Categories.Add(category);
            context.Items.Add(item);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/item/{item.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var fetchedItem = await response.Content.ReadFromJsonAsync<Item>();
        Assert.Equal(item.Name, fetchedItem.Name);
    }

    [Fact]
    public async Task GetItemById_ForUnapprovedItemByOtherUser_ReturnsForbidden()
    {
        // Arrange
        var owner = await CreateUserAsync("owner_user_2");
        var otherUser = await CreateUserAsync("other_user_2");
        var token = await GetAuthTokenAsync("other_user_2"); // Log in as the other user
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var item = new Item { Id = System.Guid.NewGuid(), Name = "Private Item", UserId = owner.Id, IsApproved = false, CategoryId = System.Guid.NewGuid(), Description = "d", Request = "r" };
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
            context.Items.Add(item);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/item/{item.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
