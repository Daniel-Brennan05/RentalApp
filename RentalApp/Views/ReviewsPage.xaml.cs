using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class ReviewsPage : ContentPage
{
    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
