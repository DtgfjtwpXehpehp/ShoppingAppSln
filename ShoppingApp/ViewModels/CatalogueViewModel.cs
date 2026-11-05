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

            var fresh = await _store.GetProductsAsync();
            if (SameProducts(_allProducts, fresh))
            {
                // Same products as before: only update stock/price. The list is not rebuilt,
                // so the user keeps their scroll position when coming back from a product.
                for (var i = 0; i < fresh.Count; i++)
                {
                    _allProducts[i].Stock = fresh[i].Stock;
                    _allProducts[i].Price = fresh[i].Price;
                }
            }
            else
            {
                _allProducts = fresh;
                ApplyFilter();
            }
        });
        IsBusy = false;
    }

    [RelayCommand]
    private Task OpenCartAsync() => _navigation.GoToAsync(Routes.Cart);

    [RelayCommand]
    private Task OpenOrderHistoryAsync() => _navigation.GoToAsync(Routes.OrderHistory);

    private static bool SameProducts(List<ProductListItem> current, List<ProductListItem> fresh) =>
        current.Count == fresh.Count &&
        current.Select(p => p.ProductId).SequenceEqual(fresh.Select(p => p.ProductId));

    private void ApplyFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;

        var matches = _allProducts.Where(p =>
            term.Length == 0 ||
            p.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            p.CategoryName.Contains(term, StringComparison.OrdinalIgnoreCase));

        Products.Clear();
        foreach (var p in matches)
            Products.Add(p);
    }
}
