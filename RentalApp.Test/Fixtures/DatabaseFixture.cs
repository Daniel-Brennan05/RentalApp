using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Test.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Tools",   Slug = "tools" },
            new Category { Id = 2, Name = "Camping", Slug = "camping" }
        };
        Context.Categories.AddRange(categories);

        var users = new List<User>
        {
            new User { Id = 1, Email = "owner@example.com",    FirstName = "Alice", LastName = "Owner",
                       PasswordHash = "hash", PasswordSalt = "salt", IsActive = true },
            new User { Id = 2, Email = "borrower@example.com", FirstName = "Bob",   LastName = "Borrower",
                       PasswordHash = "hash", PasswordSalt = "salt", IsActive = true }
        };
        Context.Users.AddRange(users);

        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Power Drill", Description = "Cordless drill",
                       DailyRate = 10.00m, CategoryId = 1, OwnerId = 1, IsAvailable = true },
            new Item { Id = 2, Title = "Camping Tent", Description = "4-person tent",
                       DailyRate = 25.00m, CategoryId = 2, OwnerId = 1, IsAvailable = true }
        };
        Context.Items.AddRange(items);

        var rentals = new List<Rental>
        {
            new Rental
            {
                Id         = 1,
                ItemId     = 1,
                BorrowerId = 2,
                StartDate  = DateTime.Today.AddDays(-7),
                EndDate    = DateTime.Today.AddDays(-4),
                Status     = RentalStatus.Returned,
                TotalPrice = 30.00m
            }
        };
        Context.Rentals.AddRange(rentals);

        var reviews = new List<Review>
        {
            new Review
            {
                Id         = 1,
                RentalId   = 1,
                ReviewerId = 2,
                Rating     = 4,
                Comment    = "Great drill, worked perfectly."
            }
        };
        Context.Reviews.AddRange(reviews);

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
