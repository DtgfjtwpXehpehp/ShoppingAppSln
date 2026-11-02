using ShoppingApp.ViewModels;

namespace ShoppingApp.Views;

public partial class CataloguePage : ContentPage
{
    private readonly CatalogueViewModel _viewModel;

    public CataloguePage(CatalogueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Runs on first display AND whenever we come back (e.g. after checkout),
    // so stock levels and the cart count are always up to date.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
