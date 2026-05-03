using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// Repository for category data access using Entity Framework Core.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<Category>> GetAllAsync()
    {
        try
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading categories: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Category?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading category {id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Category?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading category with slug '{slug}': {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Category> CreateAsync(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating category: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Category category)
    {
        try
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating category {category.Id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting category {id}: {ex.Message}");
            throw;
        }
    }
}
