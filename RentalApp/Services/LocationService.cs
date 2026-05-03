namespace RentalApp.Services;

/// <summary>
/// MAUI implementation of ILocationService using the device GPS.
/// </summary>
public class LocationService : ILocationService
{
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium));
            }

            return location != null ? (location.Latitude, location.Longitude) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location: {ex.Message}");
            return null;
        }
    }
}
