using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadNearbyItemsCommand.Execute(null);
    }
}
