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

        // Count > 1 guarantees the loop ends even if the page is not on the stack.
        while (nav.NavigationStack.Count > 1 && !IsPage(nav.NavigationStack[nav.NavigationStack.Count - 1], pageName))
        {
            await nav.PopAsync(false);
        }
    }

    private static bool IsPage(Page? page, string pageName) =>
        page is not null && string.Equals(page.GetType().Name, pageName, StringComparison.OrdinalIgnoreCase);
}
