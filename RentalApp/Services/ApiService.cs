using System.Net.Http.Json;

namespace RentalApp.Services;

/// <summary>
/// REST API client stub. Currently not wired in — app uses local database.
/// Provided as an extension point for future API integration.
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PostAsync<T>(string endpoint, object body)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PutAsync<T>(string endpoint, object body)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }
}
