using EmojitClient.Maui.Framework.Navigation;

namespace EmojitClient.Maui.Framework.Base;

/// <summary>
/// Base ViewModel class providing navigation and lifecycle support.
/// </summary>
public abstract class ViewModelBase(NavigationManager navigation) : ObservableObject
{
    protected readonly NavigationManager Navigation = navigation;

    /// <summary>
    /// Called when the ViewModel is initialized or navigated to.
    /// </summary>
    public virtual Task OnNavigatedToAsync(object? parameter = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when leaving or closing the view.
    /// </summary>
    public virtual Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }
}