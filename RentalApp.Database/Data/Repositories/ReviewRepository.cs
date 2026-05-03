using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetAllAsync()
    {
        try
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Rental).ThenInclude(rn => rn.Item)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reviews: {ex.Message}");
            throw;
        }
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Rental)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading review {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Review>> GetByItemAsync(int itemId)
    {
        try
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.Rental.ItemId == itemId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reviews for item {itemId}: {ex.Message}");
            throw;
        }
    }

    public async Task<Review?> GetByRentalAsync(int rentalId)
    {
        try
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading review for rental {rentalId}: {ex.Message}");
            throw;
        }
    }

    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        try
        {
            var ratings = await _context.Reviews
                .Where(r => r.Rental.ItemId == itemId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (ratings.Count == 0)
                return 0;

            return ratings.Average();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting average rating for item {itemId}: {ex.Message}");
            throw;
        }
    }

    public async Task<Review> CreateAsync(Review review)
    {
        try
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating review: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(Review review)
    {
        try
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating review {review.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting review {id}: {ex.Message}");
            throw;
        }
    }
}
