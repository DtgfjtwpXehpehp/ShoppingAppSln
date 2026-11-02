using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

[QueryProperty(nameof(ProductId), "ProductId")]
public partial class ProductDetailsViewModel : BaseViewModel
{
    private readonly IStoreService _store;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;

    public ProductDetailsViewModel(
        IStoreService store, ISessionService session,
        INavigationService navigation, IDialogService dialogs) : base(dialogs)
    {
        _store = store;
        _session = session;
        _navigation = navigation;
    }

    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Remaining))]
    private Product? _product;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    /// <summary>Units of this product already in the current customer's cart.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Remaining))]
    private int _inCart;

    [ObservableProperty]
    private string _quantityText = "1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>How many more units can still be added.</summary>
    public int Remaining => Math.Max(0, (Product?.StockQuantity ?? 0) - InCart);

    partial void OnProductIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var customer = _session.CurrentCustomer;
        if (customer is null || ProductId <= 0) return;

        await RunSafeAsync(async () =>
        {
            var product = await _store.GetProductAsync(ProductId);
            if (product is null) return;

            CategoryName = await _store.GetCategoryNameAsync(product.CategoryId);
            InCart = await _store.GetCartQuantityAsync(customer.CustomerId, product.ProductId);
            Product = product;
        });
    }

    [RelayCommand]
    private async Task AddToCartAsync()
    {
        ErrorMessage = string.Empty;

        var customer = _session.CurrentCustomer;
        var product = Product;
        if (customer is null || product is null) return;
        if (IsBusy) return;

        // 1) Input validation (whole number > 0)
        var text = QuantityText?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            ErrorMessage = "Please enter a quantity.";
            return;
        }
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
        {
            ErrorMessage = "Quantity must be a whole number (e.g. 1, 2, 3).";
            return;
        }
        if (quantity <= 0)
        {
            ErrorMessage = "Quantity must be greater than zero.";
            return;
        }

        IsBusy = true;
        await RunSafeAsync(async () =>
        {
            // 2) Stock validation, counting what is already in the cart.
            //    The service repeats this check inside a transaction.
            var result = await _store.AddToCartAsync(customer.CustomerId, product.ProductId, quantity);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                InCart = await _store.GetCartQuantityAsync(customer.CustomerId, product.ProductId);
                return;
            }

            InCart = await _store.GetCartQuantityAsync(customer.CustomerId, product.ProductId);
            QuantityText = "1";
            await Dialogs.AlertAsync("Added to cart", result.Message);
        });
        IsBusy = false;
    }

    [RelayCommand]
    private Task ViewCartAsync() => _navigation.GoToAsync(Routes.Cart);
}
