using EmojitClient.Maui.Enums;
using EmojitClient.Maui.Framework.Base;
using EmojitClient.Maui.Framework.Navigation;
using EmojitClient.Maui.Framework.Services;
using System.Windows.Input;

namespace EmojitClient.Maui.ViewModels;

/// <summary>
/// ViewModel for the solo game stats summary page.
/// Displays score, rounds, difficulty, and navigation options.
/// </summary>
public partial class StatsSoloViewModel : ViewModelBase
{
    private int score;
    private int rounds;
    private DifficultyLevel difficulty;

    public int Score { get => score; set => SetProperty(ref score, value); }
    public int Rounds { get => rounds; set => SetProperty(ref rounds, value); }
    public DifficultyLevel Difficulty { get => difficulty; set => SetProperty(ref difficulty, value); }

    private readonly ISoundService soundService;

    public ICommand ReplayCommand { get; }
    public ICommand ReturnMenuCommand { get; }

    public StatsSoloViewModel(NavigationManager navigation, ISoundService soundService) : base(navigation)
    {
        this.soundService = soundService;
        ReplayCommand = new Command(async () => await ReplayAsync());
        ReturnMenuCommand = new Command(async () => await ReturnMenuAsync());
    }

    public override async Task OnNavigatedToAsync(object? parameter)
    {
        if (parameter != null)
        {
            Type type = parameter.GetType();

            Score = (int)(type.GetProperty("Score")?.GetValue(parameter) ?? 0);
            Rounds = (int)(type.GetProperty("Rounds")?.GetValue(parameter) ?? 0);
            Difficulty = (DifficultyLevel)(type.GetProperty("Difficulty")?.GetValue(parameter) ?? DifficultyLevel.Easy);
        }

        await Task.CompletedTask;
    }

    private async Task ReplayAsync()
    {
        soundService.PlaySfx("click");
        await NavigationManager.NavigateToRootAsync();
        await Navigation.NavigateToAsync<ConfigSoloViewModel>();
        soundService.StopBgm();
        soundService.PlayBgm("menu");
    }

    private async Task ReturnMenuAsync()
    {

        soundService.PlaySfx("click");
        await NavigationManager.NavigateToRootAsync();
        soundService.StopBgm();
        soundService.PlayBgm("menu");
    }
}