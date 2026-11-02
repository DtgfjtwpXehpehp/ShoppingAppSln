using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

public partial class CartViewModel : BaseViewModel
{
    private readonly IStoreService _store;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;

    public CartViewModel(
        IStoreService store, ISessionService session,
        INavigationService navigation, IDialogService dialogs) : base(dialogs)
    {
        _store = store;
        _session = session;
        _navigation = navigation;
    }

    public ObservableCollection<CartLine> Lines { get; } = new();

    [ObservableProperty]
    private decimal _cartTotal;

    /// <summary>False when the cart is empty - this also disables the Checkout button.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckoutCommand))]
    private bool _hasItems;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var customer = _session.CurrentCustomer;
        if (customer is null) return;

        await RunSafeAsync(async () =>
        {
            var lines = await _store.GetCartLinesAsync(customer.CustomerId);

            Lines.Clear();
            foreach (var line in lines)
                Lines.Add(line);

            Refresh();
        });
    }

    [RelayCommand]
    private async Task RemoveAsync(CartLine? line)
    {
        if (line is null) return;

        await RunSafeAsync(async () =>
        {
            await _store.RemoveCartItemAsync(line.CartItemId);
            Lines.Remove(line);
            Refresh();
        });
    }

    [RelayCommand(CanExecute = nameof(HasItems))]
    private async Task CheckoutAsync()
    {
        var customer = _session.CurrentCustomer;
        if (customer is null) return;

        if (Lines.Count == 0)
        {
            await Dialogs.AlertAsync("Cart is empty", "Add something to your cart before checking out.");
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        await RunSafeAsync(async () =>
        {
            var result = await _store.CheckoutAsync(customer.CustomerId);

            if (!result.Success)
            {
                await Dialogs.AlertAsync("Checkout failed", result.Message);
                await LoadAsync();   // show the cart as it really is now
                return;
            }

            await Dialogs.AlertAsync(
                "Order confirmed",
                $"Thank you, {customer.FirstName}! Order #{result.OrderId} for R{result.Total:N2} has been placed.");

            // Back to the catalogue; its OnAppearing reloads stock levels and the cart count.
            await _navigation.PopToAsync(Routes.Catalogue);
        });
        IsBusy = false;
    }

    private void Refresh()
    {
        CartTotal = Lines.Sum(l => l.LineTotal);
        HasItems = Lines.Count > 0;
    }
}
