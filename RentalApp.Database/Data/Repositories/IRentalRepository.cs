using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    Task<List<Rental>> GetByBorrowerAsync(int borrowerId);
    Task<List<Rental>> GetByOwnerAsync(int ownerId);
    Task<List<Rental>> GetByItemAsync(int itemId);
    Task UpdateStatusAsync(int rentalId, string status);
}
