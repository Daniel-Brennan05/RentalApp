using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class CreateItemPage : ContentPage
{
    private readonly CreateItemViewModel _viewModel;

    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.InitializeAsync();
    }
}
