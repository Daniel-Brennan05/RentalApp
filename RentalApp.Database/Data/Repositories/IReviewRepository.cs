using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByItemAsync(int itemId);
    Task<Review?> GetByRentalAsync(int rentalId);
    Task<double> GetAverageRatingAsync(int itemId);
}
