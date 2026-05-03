using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

Console.WriteLine("Running migrations...");
using var context = new AppDbContext();
context.Database.Migrate();
Console.WriteLine("Migrations complete.");

SeedData(context);

static void SeedData(AppDbContext context)
{
    var allCategories = new[]
    {
        new Category { Name = "Tools",                  Slug = "tools" },
        new Category { Name = "Power Tools",            Slug = "power-tools" },
        new Category { Name = "Camping Gear",           Slug = "camping" },
        new Category { Name = "Sports Equipment",       Slug = "sports" },
        new Category { Name = "Bikes & Cycling",        Slug = "bikes" },
        new Category { Name = "Water Sports",           Slug = "water-sports" },
        new Category { Name = "Winter Sports",          Slug = "winter-sports" },
        new Category { Name = "Electronics",            Slug = "electronics" },
        new Category { Name = "Photography",            Slug = "photography" },
        new Category { Name = "Audio & Music",          Slug = "audio-music" },
        new Category { Name = "Musical Instruments",    Slug = "instruments" },
        new Category { Name = "Board Games",            Slug = "board-games" },
        new Category { Name = "Video Games",            Slug = "video-games" },
        new Category { Name = "Garden & Outdoor",       Slug = "garden" },
        new Category { Name = "DIY & Home Improvement", Slug = "diy" },
        new Category { Name = "Kitchen & Cooking",      Slug = "kitchen" },
        new Category { Name = "Party Supplies",         Slug = "party" },
        new Category { Name = "Baby & Kids",            Slug = "baby-kids" },
        new Category { Name = "Clothing & Costumes",    Slug = "clothing" },
        new Category { Name = "Books & Media",          Slug = "books" },
        new Category { Name = "Other",                  Slug = "other" },
    };

    var existingSlugs = context.Categories.Select(c => c.Slug).ToHashSet();
    var toAdd = allCategories.Where(c => !existingSlugs.Contains(c.Slug)).ToList();
    if (toAdd.Count > 0)
    {
        Console.WriteLine($"Seeding {toAdd.Count} new categories...");
        context.Categories.AddRange(toAdd);
        context.SaveChanges();
        Console.WriteLine("Categories seeded.");
    }

    if (!context.Roles.Any())
    {
        Console.WriteLine("Seeding roles...");
        context.Roles.AddRange(
            new Role { Name = "Admin", Description = "Administrator", IsDefault = false },
            new Role { Name = "User",  Description = "Standard user",  IsDefault = true  }
        );
        context.SaveChanges();
        Console.WriteLine("Roles seeded.");
    }

    if (!context.Users.Any())
    {
        Console.WriteLine("Seeding sample users...");

        var userRole = context.Roles.First(r => r.IsDefault);

        var sampleUsers = new[]
        {
            new { FirstName = "Alice", LastName = "Smith",   Email = "alice@example.com" },
            new { FirstName = "Bob",   LastName = "Jones",   Email = "bob@example.com"   },
            new { FirstName = "Carol", LastName = "Williams",Email = "carol@example.com" },
        };

        foreach (var u in sampleUsers)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt(11);
            var hash = BCrypt.Net.BCrypt.HashPassword("Password1!", salt);
            var user = new User
            {
                FirstName    = u.FirstName,
                LastName     = u.LastName,
                Email        = u.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow,
            };
            context.Users.Add(user);
            context.SaveChanges();
            context.UserRoles.Add(new UserRole(user.Id, userRole.Id));
            context.SaveChanges();
        }

        Console.WriteLine("Sample users seeded (password: Password1!)");
    }

    if (!context.Items.Any())
    {
        Console.WriteLine("Seeding sample items...");

        var alice = context.Users.First(u => u.Email == "alice@example.com");
        var bob   = context.Users.First(u => u.Email == "bob@example.com");

        var tools       = context.Categories.First(c => c.Slug == "tools");
        var powerTools  = context.Categories.First(c => c.Slug == "power-tools");
        var camping     = context.Categories.First(c => c.Slug == "camping");
        var sports      = context.Categories.First(c => c.Slug == "sports");
        var electronics = context.Categories.First(c => c.Slug == "electronics");
        var boardGames  = context.Categories.First(c => c.Slug == "board-games");
        var garden      = context.Categories.First(c => c.Slug == "garden");
        var kitchen     = context.Categories.First(c => c.Slug == "kitchen");

        context.Items.AddRange(
            new Item { Title = "Bosch Cordless Drill",       Description = "18V drill with two batteries and carry case. Great condition.",  DailyRate = 8.00m,  CategoryId = powerTools.Id,  OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "Pressure Washer",            Description = "Kärcher K4 pressure washer. Ideal for patios and driveways.",     DailyRate = 15.00m, CategoryId = powerTools.Id,  OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "Ladder (6-step)",            Description = "Aluminium step ladder, holds up to 150 kg.",                      DailyRate = 5.00m,  CategoryId = tools.Id,       OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "Tent (4-person)",            Description = "Coleman 4-man tent. Waterproof, easy to pitch.",                  DailyRate = 12.00m, CategoryId = camping.Id,     OwnerId = bob.Id,   IsAvailable = true  },
            new Item { Title = "Camping Stove",              Description = "Two-burner gas stove. Gas canister not included.",                DailyRate = 4.00m,  CategoryId = camping.Id,     OwnerId = bob.Id,   IsAvailable = true  },
            new Item { Title = "Sleeping Bag (-5°C rated)",  Description = "Suitable for 3-season camping. Machine washable.",               DailyRate = 3.50m,  CategoryId = camping.Id,     OwnerId = bob.Id,   IsAvailable = true  },
            new Item { Title = "Road Bike",                  Description = "Trek Domane AL2. Size M. Helmet included.",                       DailyRate = 18.00m, CategoryId = sports.Id,      OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "Kayak (single)",             Description = "Sit-on-top kayak with paddle. Perfect for flat water.",           DailyRate = 25.00m, CategoryId = sports.Id,      OwnerId = bob.Id,   IsAvailable = true  },
            new Item { Title = "Projector & Screen",         Description = "Full HD projector with 100\" screen. Great for movie nights.",    DailyRate = 20.00m, CategoryId = electronics.Id, OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "DJI Mini 3 Drone",           Description = "Camera drone with 2 batteries. Returns the same day.",            DailyRate = 35.00m, CategoryId = electronics.Id, OwnerId = bob.Id,   IsAvailable = false },
            new Item { Title = "Catan Board Game",           Description = "Complete set including Seafarers expansion.",                     DailyRate = 2.50m,  CategoryId = boardGames.Id,  OwnerId = alice.Id, IsAvailable = true  },
            new Item { Title = "Lawnmower (electric)",       Description = "Flymo corded mower. Cuts up to 300 m². Cable included.",          DailyRate = 6.00m,  CategoryId = garden.Id,      OwnerId = bob.Id,   IsAvailable = true  },
            new Item { Title = "Pasta Machine",              Description = "Marcato Atlas 150. Makes fresh pasta up to 150mm wide.",          DailyRate = 3.00m,  CategoryId = kitchen.Id,     OwnerId = alice.Id, IsAvailable = true  }
        );
        context.SaveChanges();
        Console.WriteLine("Sample items seeded.");
    }
}
