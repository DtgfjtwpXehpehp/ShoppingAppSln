using ShoppingApp.ViewModels;

namespace ShoppingApp.Views;

public partial class ProductDetailsPage : ContentPage
{
    private readonly ProductDetailsViewModel _viewModel;

    public ProductDetailsPage(ProductDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Refresh "in your cart" when returning from the cart screen.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
