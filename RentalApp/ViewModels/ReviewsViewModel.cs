using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private int newRating = 5;

    [ObservableProperty]
    private string newComment = string.Empty;

    [ObservableProperty]
    private bool canSubmitReview;

    [ObservableProperty]
    private string averageRating = string.Empty;

    public ReviewsViewModel(
        IReviewRepository reviewRepository,
        IRentalRepository rentalRepository,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _reviewRepository = reviewRepository;
        _rentalRepository = rentalRepository;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Reviews";
    }

    partial void OnItemIdChanged(int value) => LoadReviewsCommand.Execute(null);

    [RelayCommand]
    public async Task LoadReviewsAsync()
    {
        if (ItemId <= 0 || IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var reviewList = await _reviewRepository.GetByItemAsync(ItemId);

            Reviews.Clear();
            foreach (var review in reviewList)
            {
                Reviews.Add(review);
            }

            var avg = await _reviewRepository.GetAverageRatingAsync(ItemId);
            AverageRating = avg > 0 ? $"★ {avg:F1} / 5" : "No ratings yet";

            await CheckCanSubmitReviewAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (!CanSubmitReview || IsBusy) return;

        var currentUser = _authService.CurrentUser;
        if (currentUser == null) return;

        try
        {
            IsBusy = true;
            ClearError();

            var completedRentals = await _rentalRepository.GetByBorrowerAsync(currentUser.Id);
            var rental = completedRentals
                .FirstOrDefault(r => r.ItemId == ItemId && r.Status == RentalStatus.Returned);

            if (rental == null)
            {
                SetError("You must have completed a rental for this item to leave a review.");
                return;
            }

            var review = new Review
            {
                RentalId   = rental.Id,
                ReviewerId = currentUser.Id,
                Rating     = NewRating,
                Comment    = NewComment.Trim()
            };

            await _reviewRepository.CreateAsync(review);

            NewRating  = 5;
            NewComment = string.Empty;

            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit review: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateBackAsync() =>
        await _navigationService.NavigateBackAsync();

    private async Task CheckCanSubmitReviewAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            CanSubmitReview = false;
            return;
        }

        var rentals = await _rentalRepository.GetByBorrowerAsync(currentUser.Id);
        CanSubmitReview = rentals.Any(r =>
            r.ItemId == ItemId &&
            r.Status == RentalStatus.Returned &&
            r.Review == null);
    }
}
