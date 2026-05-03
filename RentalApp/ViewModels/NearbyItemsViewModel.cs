using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> nearbyItems = new();

    [ObservableProperty]
    private string locationStatus = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private Item? selectedItem;

    public NearbyItemsViewModel(
        IItemRepository itemRepository,
        ILocationService locationService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _locationService = locationService;
        _navigationService = navigationService;
        Title = "Nearby Items";
    }

    partial void OnSelectedItemChanged(Item? value)
    {
        if (value == null) return;
        _ = SelectItemAsync(value);
        SelectedItem = null;
    }

    [RelayCommand]
    public async Task LoadNearbyItemsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var location = await _locationService.GetCurrentLocationAsync();

            List<Item> items;

            if (location.HasValue)
            {
                items = await _itemRepository.GetNearbyAsync(
                    location.Value.Latitude,
                    location.Value.Longitude,
                    radiusKm: 5.0);

                if (items.Count > 0)
                {
                    LocationStatus = $"Showing items within 5km of your location";
                }
                else
                {
                    items = await _itemRepository.GetAvailableAsync();
                    LocationStatus = "No items within 5km — showing all available items";
                }
            }
            else
            {
                LocationStatus = "Location unavailable — showing all available items";
                items = await _itemRepository.GetAvailableAsync();
            }

            NearbyItems.Clear();
            foreach (var item in items)
            {
                NearbyItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load nearby items: {ex.Message}");
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
        await LoadNearbyItemsAsync();
    }

    private async Task SelectItemAsync(Item item)
    {
        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }
}
