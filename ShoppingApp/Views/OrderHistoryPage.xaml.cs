using ShoppingApp.ViewModels;

namespace ShoppingApp.Views;

public partial class OrderHistoryPage : ContentPage
{
    private readonly OrderHistoryViewModel _viewModel;

    public OrderHistoryPage(OrderHistoryViewModel viewModel)
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
