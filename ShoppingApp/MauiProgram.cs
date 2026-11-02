using ShoppingApp.Services;
using ShoppingApp.ViewModels;
using ShoppingApp.Views;

namespace ShoppingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services (one shared instance each)
        builder.Services.AddSingleton<IStoreService, StoreService>();
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();

        // View models + pages (new instance per navigation)
        builder.Services.AddTransient<CustomerSelectViewModel>();
        builder.Services.AddTransient<CatalogueViewModel>();
        builder.Services.AddTransient<ProductDetailsViewModel>();
        builder.Services.AddTransient<CartViewModel>();

        builder.Services.AddTransient<CustomerSelectPage>();
        builder.Services.AddTransient<CataloguePage>();
        builder.Services.AddTransient<ProductDetailsPage>();
        builder.Services.AddTransient<CartPage>();

        return builder.Build();
    }
}
