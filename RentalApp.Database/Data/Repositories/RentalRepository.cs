using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// Repository for rental data access using Entity Framework Core.
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetAllAsync()
    {
        try
        {
            return await _context.Rentals
                .Include(r => r.Item)
                .Include(r => r.Borrower)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rentals: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Rental?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Rentals
                .Include(r => r.Item).ThenInclude(i => i.Owner)
                .Include(r => r.Borrower)
                .Include(r => r.Review)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rental {id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetByBorrowerAsync(int borrowerId)
    {
        try
        {
            return await _context.Rentals
                .Include(r => r.Item).ThenInclude(i => i.Category)
                .Include(r => r.Item).ThenInclude(i => i.Owner)
                .Where(r => r.BorrowerId == borrowerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rentals for borrower {borrowerId}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetByOwnerAsync(int ownerId)
    {
        try
        {
            return await _context.Rentals
                .Include(r => r.Item)
                .Include(r => r.Borrower)
                .Where(r => r.Item.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rentals for owner {ownerId}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetByItemAsync(int itemId)
    {
        try
        {
            return await _context.Rentals
                .Include(r => r.Borrower)
                .Where(r => r.ItemId == itemId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rentals for item {itemId}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Rental> CreateAsync(Rental rental)
    {
        try
        {
            rental.CreatedAt = DateTime.UtcNow;
            rental.UpdatedAt = DateTime.UtcNow;
            rental.Status = RentalStatus.Requested;
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return rental;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating rental: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Rental rental)
    {
        try
        {
            rental.UpdatedAt = DateTime.UtcNow;
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating rental {rental.Id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(int rentalId, string status)
    {
        try
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental != null)
            {
                rental.Status = status;
                rental.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating status for rental {rentalId}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        try
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting rental {id}: {ex.Message}");
            throw;
        }
    }
}
