using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private string estimatedTotal = string.Empty;

    [ObservableProperty]
    private bool isOwner;

    [ObservableProperty]
    private string averageRating = string.Empty;

    public ItemDetailViewModel(
        IItemRepository itemRepository,
        IReviewRepository reviewRepository,
        IRentalService rentalService,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _reviewRepository = reviewRepository;
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    partial void OnItemIdChanged(int value) =>
        LoadItemCommand.Execute(null);

    partial void OnStartDateChanged(DateTime value) => UpdateEstimatedTotal();
    partial void OnEndDateChanged(DateTime value) => UpdateEstimatedTotal();

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (ItemId <= 0 || IsBusy) return;
        try
        {
            IsBusy = true;
            ClearError();
            Item = await _itemRepository.GetByIdAsync(ItemId);
            if (Item == null)
            {
                SetError("Item not found.");
                return;
            }
            Title = Item.Title;
            IsOwner = _authService.CurrentUser?.Id == Item.OwnerId;
            UpdateEstimatedTotal();

            var avg = await _reviewRepository.GetAverageRatingAsync(ItemId);
            AverageRating = avg > 0 ? $"★ {avg:F1} / 5" : "No ratings yet";
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (Item == null || IsBusy) return;

        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            SetError("You must be logged in to request a rental.");
            return;
        }

        if (IsOwner)
        {
            SetError("You cannot rent your own item.");
            return;
        }

        if (StartDate >= EndDate)
        {
            SetError("End date must be after start date.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            await _rentalService.RequestRentalAsync(Item.Id, currentUser.Id, StartDate, EndDate);

            await Application.Current!.MainPage!.DisplayAlert(
                "Request Sent",
                "Your rental request has been sent to the owner.",
                "OK");

            await _navigationService.NavigateToAsync("RentalsPage");
        }
        catch (Exception ex)
        {
            SetError(ex.InnerException?.Message ?? ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToEditAsync()
    {
        if (ItemId > 0)
            await _navigationService.NavigateToAsync($"CreateItemPage?itemId={ItemId}");
    }

    [RelayCommand]
    private async Task NavigateBackAsync() =>
        await _navigationService.NavigateBackAsync();

    private void UpdateEstimatedTotal()
    {
        if (Item == null || StartDate >= EndDate)
        {
            EstimatedTotal = string.Empty;
            return;
        }
        var days = Math.Max(1, (EndDate - StartDate).Days);
        EstimatedTotal = $"£{Item.DailyRate * days:F2} ({days} day{(days == 1 ? "" : "s")})";
    }
}
