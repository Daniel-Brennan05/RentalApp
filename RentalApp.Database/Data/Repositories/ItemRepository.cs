using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items: {ex.Message}");
            throw;
        }
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .Include(i => i.Reviews)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading item {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Item>> GetByOwnerAsync(int ownerId)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Where(i => i.OwnerId == ownerId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items for owner {ownerId}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Item>> GetAvailableAsync()
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .Where(i => i.IsAvailable)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading available items: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Item>> GetByCategoryAsync(string categorySlug)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .Where(i => i.Category.Slug == categorySlug && i.IsAvailable)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items for category '{categorySlug}': {ex.Message}");
            throw;
        }
    }

    public async Task<List<Item>> GetNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        try
        {
            var point = new Point(longitude, latitude) { SRID = 4326 };
            var radiusMetres = radiusKm * 1000;

            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .Where(i => i.IsAvailable && i.Location != null
                         && i.Location.IsWithinDistance(point, radiusMetres))
                .OrderBy(i => i.Location!.Distance(point))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading nearby items: {ex.Message}");
            throw;
        }
    }

    public async Task<Item> CreateAsync(Item item)
    {
        try
        {
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating item: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(Item item)
    {
        try
        {
            item.UpdatedAt = DateTime.UtcNow;
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating item {item.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.IsAvailable = false;
                item.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting item {id}: {ex.Message}");
            throw;
        }
    }
}
