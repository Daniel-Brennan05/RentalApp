using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RentalApp.Services;

/// <summary>
/// Authentication service that uses the shared SET09102 REST API.
/// Obtains and stores a JWT token, then syncs the user into the local
/// PostgreSQL database so that items and rentals continue to work offline.
/// </summary>
public class ApiAuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    private string? _token;
    private DateTime? _tokenExpiresAt;
    private User? _currentUser;

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null && !IsTokenExpired;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => IsAuthenticated ? new List<string> { "User" } : new();

    private bool IsTokenExpired =>
        _tokenExpiresAt.HasValue && DateTime.UtcNow >= _tokenExpiresAt.Value;

    public ApiAuthenticationService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/token",
                new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiError>();
                return new AuthenticationResult(false, err?.Message ?? "Invalid email or password");
            }

            var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _token = tokenData!.Token;
            _tokenExpiresAt = DateTime.Parse(tokenData.ExpiresAt);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            // Get full profile from API
            var meResponse = await _httpClient.GetAsync("/users/me");
            if (!meResponse.IsSuccessStatusCode)
                return new AuthenticationResult(false, "Could not retrieve user profile");

            var profile = await meResponse.Content.ReadFromJsonAsync<ApiUserProfile>();

            // Sync API user into local PostgreSQL so OwnerId/BorrowerId FKs work
            _currentUser = await SyncUserToLocalDbAsync(profile!);

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(
        string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/register",
                new { firstName, lastName, email, password });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiError>();
                return new AuthenticationResult(false, err?.Message ?? "Registration failed");
            }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        _token = null;
        _tokenExpiresAt = null;
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public bool HasRole(string roleName) =>
        IsAuthenticated && string.Equals(roleName, "User", StringComparison.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) => roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) => roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword) =>
        Task.FromResult(false);

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<User> SyncUserToLocalDbAsync(ApiUserProfile profile)
    {
        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == profile.Email);

        if (existing != null)
        {
            existing.FirstName = profile.FirstName;
            existing.LastName  = profile.LastName;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var newUser = new User
        {
            FirstName    = profile.FirstName,
            LastName     = profile.LastName,
            Email        = profile.Email,
            PasswordHash = "API_USER",
            PasswordSalt = "API_USER",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    // ── API response DTOs ────────────────────────────────────────────────────

    private record TokenResponse(
        [property: JsonPropertyName("token")]     string Token,
        [property: JsonPropertyName("expiresAt")] string ExpiresAt,
        [property: JsonPropertyName("userId")]    int    UserId);

    private record ApiUserProfile(
        [property: JsonPropertyName("id")]        int    Id,
        [property: JsonPropertyName("email")]     string Email,
        [property: JsonPropertyName("firstName")] string FirstName,
        [property: JsonPropertyName("lastName")]  string LastName);

    private record ApiError(
        [property: JsonPropertyName("error")]   string Error,
        [property: JsonPropertyName("message")] string Message);
}
