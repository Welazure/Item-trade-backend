using Microsoft.EntityFrameworkCore;
using Trading.Models;

namespace Trading.Context;

public class TradeContext : DbContext
{
    public TradeContext(DbContextOptions<TradeContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Item relationship
        modelBuilder.Entity<Item>()
            .HasOne(i => i.User)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category - Item relationship
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Item - Media relationship
        modelBuilder.Entity<Media>()
            .HasOne(m => m.Item)
            .WithMany(i => i.Media)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking - Item relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Item)
            .WithMany(i => i.Bookings)
            .HasForeignKey(b => b.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking - User (Booker) relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.BookerUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.BookerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Electronics" },
            new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Furniture" },
            new Category { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Books" },
            new Category { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Clothing" },
            new Category { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Sports & Outdoors" },
            new Category { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Toys & Games" },
            new Category { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Home & Garden" },
            new Category { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Other" }
        );
    }
}