using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

public partial class CustomerSelectViewModel : BaseViewModel
{
    private readonly IStoreService _store;
    private readonly ISessionService _session;
    private readonly INavigationService _navigation;

    public CustomerSelectViewModel(
        IStoreService store, ISessionService session,
        INavigationService navigation, IDialogService dialogs) : base(dialogs)
    {
        _store = store;
        _session = session;
        _navigation = navigation;
    }

    private List<Customer> _allCustomers = new();

    public ObservableCollection<Customer> Customers { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;
        var matches = _allCustomers.Where(c =>
            term.Length == 0 ||
            c.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            c.EmailAddress.Contains(term, StringComparison.OrdinalIgnoreCase));

        Customers.Clear();
        foreach (var c in matches)
            Customers.Add(c);
    }

    [ObservableProperty]
    private Customer? _selectedCustomer;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value is not null)
            _ = OpenCatalogueAsync(value);
    }

    private async Task OpenCatalogueAsync(Customer customer)
    {
        _session.CurrentCustomer = customer;
        await _navigation.GoToAsync(Routes.Catalogue);
        SelectedCustomer = null;   // so the same customer can be tapped again later
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        await RunSafeAsync(async () =>
        {
            _session.CurrentCustomer = null;
            _allCustomers = await _store.GetCustomersAsync();
            ApplyFilter();
        });
        IsBusy = false;
    }
}
