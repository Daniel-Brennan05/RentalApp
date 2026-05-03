namespace RentalApp.Services;

public interface ILocationService
{
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}
