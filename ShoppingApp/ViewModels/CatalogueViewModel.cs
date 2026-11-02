using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

public partial class CatalogueViewModel : BaseViewModel
{
    private readonly IStoreService _store;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;

    private List<ProductListItem> _allProducts = new();

    public CatalogueViewModel(
        IStoreService store, ISessionService session,
        INavigationService navigation, IDialogService dialogs) : base(dialogs)
    {
        _store = store;
        _session = session;
        _navigation = navigation;
    }

    public ObservableCollection<ProductListItem> Products { get; } = new();

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private int _cartCount;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ProductListItem? _selectedProduct;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedProductChanged(ProductListItem? value)
    {
        if (value is not null)
            _ = OpenProductAsync(value);
    }

    private async Task OpenProductAsync(ProductListItem product)
    {
        await _navigation.GoToAsync($"{Routes.ProductDetails}?ProductId={product.ProductId}");
        SelectedProduct = null;
    }

    /// <summary>Called every time the page appears, so stock and cart count are always fresh.</summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        var customer = _session.CurrentCustomer;
        if (customer is null) return;

        if (IsBusy) return;
        IsBusy = true;
        await RunSafeAsync(async () =>
        {
            CustomerName = customer.FullName;
            CartCount = await _store.GetCartCountAsync(customer.CustomerId);
            _allProducts = await _store.GetProductsAsync();
            ApplyFilter();
        });
        IsBusy = false;
    }

    [RelayCommand]
    private Task OpenCartAsync() => _navigation.GoToAsync(Routes.Cart);

    private void ApplyFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;

        var matches = _allProducts.Where(p =>
            term.Length == 0 ||
            p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            p.CategoryName.Contains(term, StringComparison.OrdinalIgnoreCase));

        Products.Clear();
        foreach (var p in matches)
            Products.Add(p);
    }
}
