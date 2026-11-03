using ShoppingApp.Services;
using ShoppingApp.Views;

namespace ShoppingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Routes.Catalogue, typeof(CataloguePage));
        Routing.RegisterRoute(Routes.ProductDetails, typeof(ProductDetailsPage));
        Routing.RegisterRoute(Routes.Cart, typeof(CartPage));
        Routing.RegisterRoute(Routes.OrderHistory, typeof(OrderHistoryPage));
    }
}
