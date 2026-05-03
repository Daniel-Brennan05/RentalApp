using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmNewPassword = string.Empty;

    [ObservableProperty]
    private bool isChangingPassword;

    public ProfileViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Profile";

        LoadUserData();
    }

    private void LoadUserData()
    {
        CurrentUser = _authService.CurrentUser;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy)
            return;

        if (!ValidatePasswordChange())
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Password changed successfully!", "OK");
                ClearPasswordFields();
                IsChangingPassword = false;
            }
            else
            {
                SetError("Failed to change password. Please check your current password.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Password change failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void TogglePasswordChangeMode()
    {
        IsChangingPassword = !IsChangingPassword;
        if (!IsChangingPassword)
        {
            ClearPasswordFields();
            ClearError();
        }
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    private bool ValidatePasswordChange()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            SetError("Current password is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            SetError("New password is required");
            return false;
        }

        if (NewPassword.Length < 6)
        {
            SetError("New password must be at least 6 characters long");
            return false;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            SetError("New passwords do not match");
            return false;
        }

        if (CurrentPassword == NewPassword)
        {
            SetError("New password must be different from current password");
            return false;
        }

        return true;
    }

    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}
