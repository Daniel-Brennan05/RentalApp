using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data;

public class AppDbContext : DbContext
{

    public AppDbContext()
    { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("RentalApp.Database.appsettings.json")
                ?? throw new InvalidOperationException("Embedded appsettings.json not found in RentalApp.Database assembly.");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            connectionString = config.GetConnectionString("DevelopmentConnection");
        }

        optionsBuilder.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
    }

    // Auth
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // Marketplace
    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasOne(i => i.Category)
                  .WithMany(c => c.Items)
                  .HasForeignKey(i => i.CategoryId);

            entity.HasOne(i => i.Owner)
                  .WithMany()
                  .HasForeignKey(i => i.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasOne(r => r.Item)
                  .WithMany(i => i.Rentals)
                  .HasForeignKey(r => r.ItemId);

            entity.HasOne(r => r.Borrower)
                  .WithMany()
                  .HasForeignKey(r => r.BorrowerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Review)
                  .WithOne(rv => rv.Rental)
                  .HasForeignKey<Review>(rv => rv.RentalId);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(rv => rv.Reviewer)
                  .WithMany()
                  .HasForeignKey(rv => rv.ReviewerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

}