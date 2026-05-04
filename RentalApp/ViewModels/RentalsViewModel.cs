using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Rental> myRequests = new();

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRequests = new();

    [ObservableProperty]
    private bool isRefreshing;

    public RentalsViewModel(
        IRentalRepository rentalRepository,
        IRentalService rentalService,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _rentalRepository = rentalRepository;
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "My Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (IsBusy) return;

        var currentUser = _authService.CurrentUser;
        if (currentUser == null) return;

        try
        {
            IsBusy = true;
            ClearError();

            var outgoing = await _rentalRepository.GetByBorrowerAsync(currentUser.Id);
            MyRequests = new ObservableCollection<Rental>(outgoing);

            var incoming = await _rentalRepository.GetByOwnerAsync(currentUser.Id);
            IncomingRequests = new ObservableCollection<Rental>(incoming);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
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
        await LoadRentalsAsync();
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        if (rental == null) return;
        try
        {
            await _rentalService.ApproveRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        if (rental == null) return;
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Reject Request",
            $"Reject rental request from {rental.Borrower?.FullName}?",
            "Reject", "Cancel");

        if (!confirm) return;

        try
        {
            await _rentalService.RejectRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task MarkOutForRentAsync(Rental rental)
    {
        if (rental == null) return;
        try
        {
            await _rentalService.StartRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task MarkReturnedAsync(Rental rental)
    {
        if (rental == null) return;
        try
        {
            await _rentalService.ReturnRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync() =>
        await _navigationService.NavigateToAsync("MainPage");

    [RelayCommand]
    private async Task NavigateToBrowseAsync() =>
        await _navigationService.NavigateToAsync("ItemsListPage");

    [RelayCommand]
    private async Task NavigateToCreateItemAsync() =>
        await _navigationService.NavigateToAsync("CreateItemPage");
}
