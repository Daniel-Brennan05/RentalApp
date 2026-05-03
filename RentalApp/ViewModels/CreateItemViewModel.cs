using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetTopologySuite.Geometries;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private bool isAvailable = true;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string locationStatus = string.Empty;

    public CreateItemViewModel(
        IItemRepository itemRepository,
        ICategoryRepository categoryRepository,
        IAuthenticationService authService,
        INavigationService navigationService,
        ILocationService locationService)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _authService = authService;
        _navigationService = navigationService;
        _locationService = locationService;
        Title = "List an Item";
    }

    partial void OnItemIdChanged(int value) => _ = InitializeAsync();

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var categoryList = await _categoryRepository.GetAllAsync();
            Categories.Clear();
            foreach (var category in categoryList)
                Categories.Add(category);

            if (ItemId > 0)
            {
                IsEditMode = true;
                Title = "Edit Listing";

                var item = await _itemRepository.GetByIdAsync(ItemId);
                if (item != null)
                {
                    ItemTitle        = item.Title;
                    Description      = item.Description ?? string.Empty;
                    DailyRateText    = item.DailyRate.ToString("F2");
                    IsAvailable      = item.IsAvailable;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId)
                                       ?? Categories.FirstOrDefault();
                }
            }
            else
            {
                IsEditMode       = false;
                Title            = "List an Item";
                SelectedCategory = Categories.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveItemAsync()
    {
        if (IsBusy) return;
        if (!ValidateForm()) return;

        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            SetError("You must be logged in.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var dailyRate = decimal.Parse(DailyRateText);

            if (IsEditMode)
            {
                var item = await _itemRepository.GetByIdAsync(ItemId);
                if (item == null) { SetError("Item not found."); return; }

                item.Title       = ItemTitle.Trim();
                item.Description = Description.Trim();
                item.DailyRate   = dailyRate;
                item.CategoryId  = SelectedCategory!.Id;
                item.IsAvailable = IsAvailable;

                await _itemRepository.UpdateAsync(item);

                await Application.Current!.MainPage!.DisplayAlert(
                    "Updated", $"'{item.Title}' has been updated.", "OK");
            }
            else
            {
                NetTopologySuite.Geometries.Point? location = null;
                if (!string.IsNullOrEmpty(LocationStatus) && LocationStatus.StartsWith("Location set"))
                {
                    var gps = await _locationService.GetCurrentLocationAsync();
                    if (gps.HasValue)
                        location = new NetTopologySuite.Geometries.Point(gps.Value.Longitude, gps.Value.Latitude) { SRID = 4326 };
                }

                var item = new Item
                {
                    Title       = ItemTitle.Trim(),
                    Description = Description.Trim(),
                    DailyRate   = dailyRate,
                    CategoryId  = SelectedCategory!.Id,
                    OwnerId     = currentUser.Id,
                    IsAvailable = true,
                    Location    = location
                };

                await _itemRepository.CreateAsync(item);

                await Application.Current!.MainPage!.DisplayAlert(
                    "Listed!", $"'{item.Title}' is now available to rent.", "OK");
            }

            await _navigationService.NavigateToAsync("ItemsListPage");
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        try
        {
            IsBusy = true;
            var location = await _locationService.GetCurrentLocationAsync();

            if (location.HasValue)
            {
                LocationStatus = $"Location set ({location.Value.Latitude:F4}, {location.Value.Longitude:F4})";
            }
            else
            {
                LocationStatus = "Could not get location. Try again.";
            }
        }
        catch (Exception ex)
        {
            LocationStatus = $"Location error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync() =>
        await _navigationService.NavigateBackAsync();

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(ItemTitle))
        {
            SetError("Title is required.");
            return false;
        }

        if (!decimal.TryParse(DailyRateText, out var rate) || rate <= 0)
        {
            SetError("Please enter a valid daily rate (e.g. 9.99).");
            return false;
        }

        if (SelectedCategory == null)
        {
            SetError("Please select a category.");
            return false;
        }

        return true;
    }
}
