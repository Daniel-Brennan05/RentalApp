using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class ItemsListPage : ContentPage
{
    private readonly ItemsListViewModel _viewModel;

    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadItemsCommand.Execute(null);
    }
}
