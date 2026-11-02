namespace ShoppingApp.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();

    /// <summary>Pops pages until the page registered under <paramref name="pageName"/> is on top.</summary>
    Task PopToAsync(string pageName);
}

public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public async Task PopToAsync(string pageName)
    {
        var nav = Shell.Current.Navigation;
        while (nav.NavigationStack.Count > 1 &&
               nav.NavigationStack[nav.NavigationStack.Count - 1]?.GetType().Name != pageName)
        {
            await nav.PopAsync(false);
        }
    }
}
