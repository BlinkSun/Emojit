using EmojitClient.Maui.Framework.Base;
using EmojitClient.Maui.Framework.Behaviors;
using EmojitClient.Maui.Framework.Navigation;
using EmojitClient.Maui.Views;
using System.Diagnostics;

namespace EmojitClient.Maui.ViewModels;

/// <summary>
/// ViewModel for the main menu page.
/// Handles navigation to available game modes.
/// </summary>
public sealed partial class LoadingViewModel : ViewModelBase
{
    private bool isNavigating;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingViewModel"/> class.
    /// </summary>
    /// <param name="navigation">Navigation manager used to handle page transitions.</param>
    public LoadingViewModel(NavigationManager navigation) : base(navigation)
    {
    }

    /// <summary>
    /// Navigates to the solo configuration page.
    /// </summary>
    private async Task GoToSoloModeAsync()
    {
        if (!CanNavigate())
            return;

        try
        {
            isNavigating = true;
            if (Application.Current?.Windows[0]?.Page is NavigationPage navPage && navPage.CurrentPage is LoadingPage loadingPage)
            {
                //await loadingPage.AnimateExitAsync();
                AnimatedPageBehavior? behavior = loadingPage.Behaviors.OfType<AnimatedPageBehavior>().FirstOrDefault();
                if (behavior != null) await behavior.AnimateExitAsync(loadingPage);
            }
            await Navigation.NavigateToAsync<MainMenuViewModel>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Error navigating to MainMenuPage: {ex.Message}");
        }
        finally
        {
            isNavigating = false;
        }
    }

    /// <summary>
    /// Prevents rapid double-tap navigation.
    /// </summary>
    private bool CanNavigate()
    {
        return !isNavigating;
    }

    /// <inheritdoc/>
    public override Task OnNavigatedToAsync(object? parameter)
    {
        // Future: Load player profile, theme, or settings here.
        return Task.CompletedTask;
    }
}