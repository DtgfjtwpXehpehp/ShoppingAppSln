namespace ShoppingApp.Services;

public interface IDialogService
{
    Task AlertAsync(string title, string message);
}

public class ShellDialogService : IDialogService
{
    // .NET 10: DisplayAlert is obsolete, DisplayAlertAsync replaces it.
    public Task AlertAsync(string title, string message) =>
        Shell.Current.DisplayAlertAsync(title, message, "OK");
}
