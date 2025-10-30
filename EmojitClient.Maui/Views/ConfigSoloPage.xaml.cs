using EmojitClient.Maui.ViewModels;

namespace EmojitClient.Maui.Views;

public partial class ConfigSoloPage : ContentPage
{
    private bool hasAnimated = false;

    public ConfigSoloPage(ConfigSoloViewModel viewModel)
    {
        InitializeComponent();

        Opacity = 0;
        TitleLabel.Opacity = 0;
        DifficultyButtons.Opacity = 0;
        BackButton.Opacity = 0;

        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
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
        if (TitleLabel != null)
        {
            await TitleLabel.FadeTo(1, 500, Easing.CubicOut);
        }

        if (DifficultyButtons != null)
        {
            await DifficultyButtons.FadeTo(1, 400, Easing.CubicOut);
        }

        if (BackButton != null)
        {
            await BackButton.FadeTo(1, 400);
        }
    }

    public async Task AnimateExitAsync()
    {
        try
        {
            List<Task> tasks = [];

            if (TitleLabel != null)
                tasks.Add(TitleLabel.FadeTo(0, 300, Easing.CubicOut));

            if (DifficultyButtons != null)
                tasks.Add(DifficultyButtons.FadeTo(0, 300, Easing.CubicIn));

            if (BackButton != null)
                tasks.Add(BackButton.FadeTo(0, 300));

            await Task.WhenAll(tasks);
        }
        catch { }
        finally
        {
            hasAnimated = false;
        }
    }
}