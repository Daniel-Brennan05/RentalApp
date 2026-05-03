using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class UserListViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    private ObservableCollection<UserListItem> _allUsers = new();

    [ObservableProperty]
    private ObservableCollection<UserListItem> filteredUsers = new();

    [ObservableProperty]
    private string selectedRoleFilter = "All";

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<string> RoleFilterOptions { get; } = new();

    public bool IsAdmin => _authenticationService.HasRole(RoleConstants.Admin);

    public UserListViewModel(AppDbContext context, INavigationService navigationService, IAuthenticationService authenticationService)
    {
        _context = context;
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        Title = "User Management";

        RoleFilterOptions.Add("All");
        foreach (var role in RoleConstants.AllRoles)
            RoleFilterOptions.Add(role);

        _ = LoadUsersAsync();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnSelectedRoleFilterChanged(string value) => ApplyFilters();

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        if (!IsAdmin)
        {
            await _navigationService.NavigateToAsync("MainPage");
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            _allUsers = new ObservableCollection<UserListItem>(users.Select(u => new UserListItem
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name).ToList(),
                RolesDisplay = string.Join(", ", u.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name))
            }));

            ApplyFilters();
        }
        catch (Exception ex)
        {
            SetError($"Error loading users: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadUsersAsync();
    }

    [RelayCommand]
    private async Task UserSelectedAsync(UserListItem user)
    {
        if (user == null) return;
        await _navigationService.NavigateToAsync($"UserDetailPage?userId={user.Id}");
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        await _navigationService.NavigateToAsync("UserDetailPage?userId=0");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    private void ApplyFilters()
    {
        var filtered = (IEnumerable<UserListItem>)_allUsers;

        if (SelectedRoleFilter != "All")
            filtered = filtered.Where(u => u.Roles.Contains(SelectedRoleFilter));

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(u =>
                u.FullName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                u.RolesDisplay.ToLower().Contains(searchLower));
        }

        FilteredUsers = new ObservableCollection<UserListItem>(filtered);
    }
}

public class UserListItem
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public string RolesDisplay { get; set; } = string.Empty;
}
