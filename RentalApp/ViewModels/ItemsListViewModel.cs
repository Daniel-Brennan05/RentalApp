using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private Item? selectedItem;

    partial void OnSelectedItemChanged(Item? value)
    {
        if (value == null) return;
        _ = SelectItemAsync(value);
        SelectedItem = null;
    }

    public ItemsListViewModel(
        IItemRepository itemRepository,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ClearError();
            var result = await _itemRepository.GetAvailableAsync();
            Items = new ObservableCollection<Item>(result);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
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
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task SelectItemAsync(Item item)
    {
        if (item == null) return;
        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }

    [RelayCommand]
    private async Task NavigateToCreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    [RelayCommand]
    private async Task NavigateToRentalsAsync()
    {
        await _navigationService.NavigateToAsync("RentalsPage");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }
}
