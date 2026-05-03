using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}
