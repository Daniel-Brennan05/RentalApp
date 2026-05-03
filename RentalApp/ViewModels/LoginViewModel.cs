using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    public LoginViewModel()
    {
        Title = "Login";
    }

    public LoginViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both email and password");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccess)
            {
                await _navigationService.NavigateToAsync("MainPage");
            }
            else
            {
                SetError(result.Message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync("RegisterPage");
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        await Application.Current.MainPage.DisplayAlert("Info", "Forgot password functionality not implemented yet", "OK");
    }
}
