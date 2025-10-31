using EmojitClient.Maui.Framework.Base;
using EmojitClient.Maui.Framework.Base.Command;
using EmojitClient.Maui.Framework.Behaviors;
using EmojitClient.Maui.Framework.Navigation;
using EmojitClient.Maui.Framework.Services;
using EmojitClient.Maui.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace EmojitClient.Maui.ViewModels;

/// <summary>
/// ViewModel for the main menu page.
/// Handles navigation to available game modes.
/// </summary>
public sealed partial class MainMenuViewModel : ViewModelBase
{
    private bool isNavigating;
    private readonly ISoundService soundService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenuViewModel"/> class.
    /// </summary>
    /// <param name="navigation">Navigation manager used to handle page transitions.</param>
    public MainMenuViewModel(NavigationManager navigation, ISoundService soundService) : base(navigation)
    {
        PlaySoloCommand = new AsyncRelayCommand(GoToSoloModeAsync, CanNavigate);
        this.soundService = soundService;
        // Future commands can be defined similarly:
        // PlayWithFriendCommand = new AsyncRelayCommand(GoToFriendModeAsync, () => false);
        // PlayOnlineCommand = new AsyncRelayCommand(GoToOnlineModeAsync, () => false);
    }

    // ========= COMMANDES =========

    /// <summary>
    /// Command for starting solo mode.
    /// </summary>
    public ICommand PlaySoloCommand { get; }

    // public ICommand PlayWithFriendCommand { get; }
    // public ICommand PlayOnlineCommand { get; }

    // ========= MÉTHODES =========

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
            soundService.PlaySfx("click");
            if (Application.Current?.Windows[0]?.Page is NavigationPage navPage && navPage.CurrentPage is MainMenuPage mainPage)
            {
                //await mainPage.AnimateExitAsync();
                //AnimatedPageBehavior? behavior = mainPage.Behaviors.OfType<AnimatedPageBehavior>().FirstOrDefault();
                //if (behavior != null) await behavior.AnimateExitAsync(mainPage);
            }
            await Navigation.NavigateToAsync<ConfigSoloViewModel>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Error navigating to ConfigSoloPage: {ex.Message}");
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

    // ========= CYCLE DE VIE =========

    /// <inheritdoc/>
    public override Task OnNavigatedToAsync(object? parameter)
    {
        // Future: Load player profile, theme, or settings here.
        return Task.CompletedTask;
    }
}