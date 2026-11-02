using ShoppingApp.ViewModels;

namespace ShoppingApp.Views;

public partial class CustomerSelectPage : ContentPage
{
    private readonly CustomerSelectViewModel _viewModel;

    public CustomerSelectPage(CustomerSelectViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
