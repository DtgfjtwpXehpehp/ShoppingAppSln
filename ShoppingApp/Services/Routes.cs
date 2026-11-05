namespace ShoppingApp.Services;

/// <summary>
/// Shell route names, shared by AppShell and the view models.
/// Each value MUST equal the page's class name (e.g. "CataloguePage"): INavigationService.PopToAsync
/// finds the page to return to by comparing the route with the page class name.
/// </summary>
public static class Routes
{
    public const string CustomerSelect = "CustomerSelectPage";
    public const string Catalogue = "CataloguePage";
    public const string ProductDetails = "ProductDetailsPage";
    public const string Cart = "CartPage";
    public const string OrderHistory = "OrderHistoryPage";
}
