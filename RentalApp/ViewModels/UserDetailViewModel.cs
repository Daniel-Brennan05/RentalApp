using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authService;

    private User? _currentUser;

    [ObservableProperty]
    private int userId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string firstName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string lastName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteUserCommand))]
    private bool isNewUser;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RoleItem> availableRoles = new();

    public string PageTitle => IsNewUser ? "Create New User" : "Edit User";
    public bool ShowPasswordFields => IsNewUser;
    public bool CanDeleteCurrentUser => !IsNewUser && _currentUser?.Id != _authService.CurrentUser?.Id;

    public UserDetailViewModel(AppDbContext context, INavigationService navigationService, IAuthenticationService authService)
    {
        _context = context;
        _navigationService = navigationService;
        _authService = authService;
    }

    partial void OnUserIdChanged(int value) => _ = LoadUserAsync();

    partial void OnIsNewUserChanged(bool value)
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(ShowPasswordFields));
        OnPropertyChanged(nameof(CanDeleteCurrentUser));
    }

    // IsBusy is declared in BaseViewModel — notify commands when it changes
    private bool CanSaveUser() =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        (!IsNewUser || (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword)));

    private bool CanDeleteUser() => !IsNewUser && CanDeleteCurrentUser;

    [RelayCommand(CanExecute = nameof(CanSaveUser))]
    private async Task SaveUserAsync()
    {
        ClearMessages();

        if (!ValidateInput())
            return;

        // Capture before CreateUserAsync flips IsNewUser to false
        var isCreating = IsNewUser;

        try
        {
            IsBusy = true;

            if (isCreating)
                await CreateUserAsync();
            else
                await UpdateUserAsync();

            SuccessMessage = isCreating ? "User created successfully!" : "User updated successfully!";

            if (isCreating)
            {
                await Task.Delay(1500);
                await BackAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Error saving user: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private async Task DeleteUserAsync()
    {
        if (_currentUser == null) return;

        var confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete user '{_currentUser.FullName}'? This action cannot be undone.",
            "Delete", "Cancel");

        if (!confirmed) return;

        try
        {
            IsBusy = true;

            _currentUser.IsActive = false;
            _currentUser.DeletedAt = DateTime.UtcNow;
            _currentUser.UpdatedAt = DateTime.UtcNow;

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == _currentUser.Id)
                .ToListAsync();

            foreach (var userRole in userRoles)
                userRole.MarkAsDeleted();

            _context.Users.Update(_currentUser);
            _context.UserRoles.UpdateRange(userRoles);
            await _context.SaveChangesAsync();

            await BackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Error deleting user: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddRoleAsync(RoleItem role)
    {
        if (_currentUser == null || role.IsAssigned) return;

        try
        {
            var userRole = new UserRole(_currentUser.Id, role.Id);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            role.IsAssigned = true;
            SuccessMessage = $"Role '{role.Name}' added successfully!";
            await Task.Delay(1500);
            ClearMessages();
        }
        catch (Exception ex)
        {
            SetError($"Error adding role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveRoleAsync(RoleItem role)
    {
        if (_currentUser == null || !role.IsAssigned) return;

        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == _currentUser.Id && ur.RoleId == role.Id && ur.IsActive);

            if (userRole != null)
            {
                userRole.MarkAsDeleted();
                _context.UserRoles.Update(userRole);
                await _context.SaveChangesAsync();

                role.IsAssigned = false;
                SuccessMessage = $"Role '{role.Name}' removed successfully!";
                await Task.Delay(1500);
                ClearMessages();
            }
        }
        catch (Exception ex)
        {
            SetError($"Error removing role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigationService.NavigateBackAsync();

    [RelayCommand]
    private async Task NavigateToDashboardAsync() => await _navigationService.NavigateToAsync("MainPage");

    private async Task LoadUserAsync()
    {
        if (!_authService.HasRole(RoleConstants.Admin))
        {
            await _navigationService.NavigateToAsync("MainPage");
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearMessages();

            var allRoles = await _context.Roles.ToListAsync();

            if (UserId == 0)
            {
                IsNewUser = true;
                _currentUser = null;
                FirstName = string.Empty;
                LastName = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsActive = true;

                AvailableRoles = new ObservableCollection<RoleItem>(
                    allRoles.Select(r => new RoleItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsAssigned = false
                    }));
            }
            else
            {
                IsNewUser = false;
                _currentUser = await _context.Users
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == UserId);

                if (_currentUser == null)
                {
                    SetError("User not found.");
                    return;
                }

                FirstName = _currentUser.FirstName;
                LastName = _currentUser.LastName;
                Email = _currentUser.Email;
                IsActive = _currentUser.IsActive;

                var assignedRoleIds = _currentUser.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.RoleId)
                    .ToList();

                AvailableRoles = new ObservableCollection<RoleItem>(
                    allRoles.Select(r => new RoleItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsAssigned = assignedRoleIds.Contains(r.Id)
                    }));
            }

            OnPropertyChanged(nameof(CanDeleteCurrentUser));
            DeleteUserCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            SetError($"Error loading user: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateUserAsync()
    {
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email.Trim());
        if (existing != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(Password, salt);

        var user = new User
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = Email.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var role in AvailableRoles.Where(r => r.IsAssigned))
            _context.UserRoles.Add(new UserRole(user.Id, role.Id));

        if (AvailableRoles.Any(r => r.IsAssigned))
            await _context.SaveChangesAsync();

        _currentUser = user;
        IsNewUser = false;
    }

    private async Task UpdateUserAsync()
    {
        if (_currentUser == null) return;

        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == Email.Trim() && u.Id != _currentUser.Id);
        if (existing != null)
            throw new InvalidOperationException("This email is already used by another user.");

        _currentUser.FirstName = FirstName.Trim();
        _currentUser.LastName = LastName.Trim();
        _currentUser.Email = Email.Trim();
        _currentUser.IsActive = IsActive;
        _currentUser.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(_currentUser);
        await _context.SaveChangesAsync();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            SetError("First name is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(LastName))
        {
            SetError("Last name is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            SetError("Email is required.");
            return false;
        }
        if (!IsValidEmail(Email.Trim()))
        {
            SetError("Please enter a valid email address.");
            return false;
        }
        if (IsNewUser)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required.");
                return false;
            }
            if (Password.Length < 6)
            {
                SetError("Password must be at least 6 characters.");
                return false;
            }
            if (Password != ConfirmPassword)
            {
                SetError("Passwords do not match.");
                return false;
            }
        }
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        const string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    private void ClearMessages()
    {
        ClearError();
        SuccessMessage = string.Empty;
    }
}

public partial class RoleItem : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isAssigned;

    partial void OnIsAssignedChanged(bool value)
    {
        OnPropertyChanged(nameof(ButtonText));
        OnPropertyChanged(nameof(ButtonColor));
    }

    public string ButtonText => IsAssigned ? "Remove" : "Add";
    public Color ButtonColor => IsAssigned ? Color.FromArgb("#dc3545") : Color.FromArgb("#28a745");
}
