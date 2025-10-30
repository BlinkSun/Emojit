using EmojitClient.Maui.ViewModels;

namespace EmojitClient.Maui.Views;

public partial class StatsSoloPage : ContentPage
{
    private bool hasAnimated = false;

    public StatsSoloPage(StatsSoloViewModel viewModel)
    {
        InitializeComponent();

        Opacity = 0;
        ScoreBoard.Opacity = 0;
        BackButtons.Opacity = 0;

        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        // Empêche toute navigation retour, y compris le geste swipe sur Android
        return true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasAnimated) return;

        try
        {
            await AnimateEntranceAsync();
        }
        catch { }
        finally
        {
            hasAnimated = true;
        }
    }

    public async Task AnimateEntranceAsync()
    {
        if (ScoreBoard != null)
        {
            await ScoreBoard.FadeTo(1, 500, Easing.CubicOut);
        }

        if (BackButtons != null)
        {
            await BackButtons.FadeTo(1, 400, Easing.CubicOut);
        }
    }

    public async Task AnimateExitAsync()
    {
        try
        {
            List<Task> tasks = [];

            if (ScoreBoard != null)
                tasks.Add(ScoreBoard.FadeTo(0, 300, Easing.CubicOut));

            if (BackButtons != null)
                tasks.Add(BackButtons.FadeTo(0, 300, Easing.CubicIn));

            await Task.WhenAll(tasks);
        }
        catch { }
        finally
        {
            hasAnimated = false;
        }
    }
}