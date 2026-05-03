using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<List<Item>> GetByOwnerAsync(int ownerId);
    Task<List<Item>> GetAvailableAsync();
    Task<List<Item>> GetByCategoryAsync(string categorySlug);
    Task<List<Item>> GetNearbyAsync(double latitude, double longitude, double radiusKm);
}
