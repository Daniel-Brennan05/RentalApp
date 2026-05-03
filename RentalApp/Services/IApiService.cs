namespace RentalApp.Services;

/// <summary>
/// Interface for the remote REST API client.
/// Currently unused — the app uses local PostgreSQL via LocalAuthenticationService.
/// Provided as an extension point for future API integration.
/// </summary>
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object body);
    Task<T?> PutAsync<T>(string endpoint, object body);
    Task DeleteAsync(string endpoint);
}
