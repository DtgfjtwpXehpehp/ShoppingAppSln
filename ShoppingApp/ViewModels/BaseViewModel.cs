using CommunityToolkit.Mvvm.ComponentModel;
using ShoppingApp.Services;

namespace ShoppingApp.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    protected BaseViewModel(IDialogService dialogs) => Dialogs = dialogs;

    protected IDialogService Dialogs { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    /// <summary>Runs <paramref name="action"/> and shows a friendly alert if it throws.</summary>
    protected async Task RunSafeAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await Dialogs.AlertAsync("Something went wrong", ex.Message);
        }
    }
}
