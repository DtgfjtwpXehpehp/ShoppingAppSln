using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

public partial class OrderHistoryViewModel : BaseViewModel
{
    private readonly IStoreService _store;
    private readonly ISessionService _session;

    public OrderHistoryViewModel(
        IStoreService store, ISessionService session, IDialogService dialogs) : base(dialogs)
    {
        _store = store;
        _session = session;
    }

    public ObservableCollection<OrderSummary> Orders { get; } = new();

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _summaryText = string.Empty;

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
            var orders = await _store.GetOrderHistoryAsync(customer.CustomerId);

            Orders.Clear();
            foreach (var order in orders)
                Orders.Add(order);

            var spent = orders.Sum(o => o.TotalAmount);
            SummaryText = orders.Count == 0
                ? "No orders yet"
                : $"{orders.Count} order{(orders.Count == 1 ? "" : "s")}  •  R{spent:N2} spent in total";
        });
        IsBusy = false;
    }
}
