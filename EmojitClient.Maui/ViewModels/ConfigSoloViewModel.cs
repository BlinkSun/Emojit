using EmojitClient.Maui.Enums;
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
/// ViewModel for the solo game configuration page.
/// Allows player to choose difficulty before starting.
/// </summary>
public partial class ConfigSoloViewModel : ViewModelBase
{
    private readonly ISoundService soundService;
    public ConfigSoloViewModel(NavigationManager navigation, ISoundService soundService) : base(navigation)
    {
        this.soundService = soundService;
        SelectDifficultyCommand = new AsyncRelayCommand<DifficultyLevel>(GoToGameAsync);
        GoBackCommand = new AsyncRelayCommand(GoBackAsync);
    }

    /// <summary>
    /// Command triggered when user selects a difficulty level.
    /// </summary>
    public ICommand SelectDifficultyCommand { get; }

    /// <summary>
    /// Command triggered when user taps the "Back" button.
    /// </summary>
    public ICommand GoBackCommand { get; }

    private async Task GoToGameAsync(DifficultyLevel difficulty)
    {
        try
        {
            soundService.PlaySfx("click");
            if (Application.Current?.Windows[0]?.Page is NavigationPage navPage && navPage.CurrentPage is ConfigSoloPage configPage)
            {
                await configPage.AnimateExitAsync();
                AnimatedPageBehavior? behavior = configPage.Behaviors.OfType<AnimatedPageBehavior>().FirstOrDefault();
                if (behavior != null) await behavior.AnimateExitAsync(configPage);
            }

            await Navigation.NavigateToAsync<PlaySoloViewModel>(difficulty);
            soundService.StopBgm();
            soundService.PlayBgm("battle");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Error navigating to PlaySoloPage: {ex.Message}");
        }
    }

    private async Task GoBackAsync()
    {
        try
        {
            soundService.PlaySfx("click");
            if (Application.Current?.Windows[0]?.Page is NavigationPage navPage && navPage.CurrentPage is ConfigSoloPage configPage)
            {
                await configPage.AnimateExitAsync();
                AnimatedPageBehavior? behavior = configPage.Behaviors.OfType<AnimatedPageBehavior>().FirstOrDefault();
                if (behavior != null) await behavior.AnimateExitAsync(configPage);
            }
            await NavigationManager.GoBackAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Error during GoBackAsync: {ex.Message}");
        }
    }

    public override Task OnNavigatedToAsync(object? parameter)
    {
        // Reset or load player preferences if needed
        return Task.CompletedTask;
    }
}
