using EmojitClient.Maui.Framework.Base;
using System.Diagnostics;

namespace EmojitClient.Maui.Framework.Navigation;

/// <summary>
/// Centralized navigation manager for MAUI apps without AppShell.
/// Supports parameter passing and dependency injection.
/// </summary>
public class NavigationManager(IServiceProvider services)
{
    private static INavigation Navigation => (Application.Current?.Windows[0].Page?.Navigation ?? throw new InvalidOperationException("Navigation stack not available. Ensure MainPage is a NavigationPage."));

    public async Task NavigateToAsync<TViewModel, TParameter>(TParameter parameter) where TViewModel : class
    {
        await NavigateToAsync<TViewModel>(parameter);
    }
    public async Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        Page page = ResolvePageForViewModel<TViewModel>();
        NavigationPage.SetHasNavigationBar(page, false);
        TViewModel viewModel = (TViewModel)page.BindingContext;

        if (viewModel is ViewModelBase vmBase) await vmBase.OnNavigatedToAsync(parameter);
        await Navigation.PushAsync(page, false);
        Debug.WriteLine($"[NAV] Navigated to {typeof(TViewModel).Name}");
    }
    public static async Task NavigateToRootAsync()
    {
        await Navigation.PopToRootAsync(false);
        Debug.WriteLine($"[NAV] Reset navigation to root.");
    }
    public static async Task GoBackAsync()
    {
        if (Navigation.NavigationStack.Count <= 1)
        {
            Debug.WriteLine("[NAV] No previous page in stack.");
            return;
        }

        await Navigation.PopAsync(false);
        Debug.WriteLine("[NAV] Navigated back.");
    }

    /// <summary>
    /// Resolve a Page instance for a ViewModel using DI.
    /// </summary>
    private Page ResolvePageForViewModel<TViewModel>() where TViewModel : class
    {
        // Find corresponding Page type
        Type viewModelType = typeof(TViewModel);
        Type? pageType = FindPageForViewModel(viewModelType)
            ?? throw new InvalidOperationException($"No page found for ViewModel {viewModelType.FullName}");

        // Create page via DI
        if (services.GetService(pageType) is not Page page)
            throw new InvalidOperationException($"Page {pageType.FullName} is not registered in DI.");

        // Ensure BindingContext is the expected ViewModel
        if (page.BindingContext is not TViewModel)
            throw new InvalidOperationException($"Page {pageType.Name} has no ViewModel of type {viewModelType.Name} as BindingContext.");

        return page;
    }
    /// <summary>
    /// Finds the Page type that matches a ViewModel type based on naming conventions.
    /// </summary>
    private static Type? FindPageForViewModel(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        string? pageName = viewModelType.FullName?
            .Replace("ViewModels", "Views")
            .Replace("ViewModel", "Page");

        return string.IsNullOrWhiteSpace(pageName) ? null : Type.GetType(pageName);
    }
}

