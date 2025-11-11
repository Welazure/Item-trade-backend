using Microsoft.EntityFrameworkCore;
using Trading.Context;

namespace Trading.Data;

public class DbInitializer
{
    /// <summary>
    /// Migrates legacy user data on application startup.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        // Create a scope to resolve services
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradeContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();

        try
        {
            // Find all users created under the old schema (where Points is 0)
            var usersToUpdate = await context.Users.Where(u => u.Points == 0).ToListAsync();

            if (usersToUpdate.Count == 0) return;
            
            logger.LogInformation("Found {UserCount} users with no points. Assigning initial 2 points...", usersToUpdate.Count);
            usersToUpdate.ForEach(user => user.Points = 2);
            await context.SaveChangesAsync();
            logger.LogInformation("Successfully updated {UserCount} users.", usersToUpdate.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during user points migration.");
        }
    }
}