using EmojitClient.Maui.Models;
using EmojitClient.Maui.ViewModels;

namespace EmojitClient.Maui.Views;

public partial class PlaySoloPage : ContentPage
{
    private bool hasAnimated = false;
    private readonly PlaySoloViewModel vm;

    public PlaySoloPage(PlaySoloViewModel viewModel)
    {
        InitializeComponent();

        Opacity = 0;
        ScoreBoard.Opacity = 0;
        StackCollection.Opacity = 0;
        PlayerCollection.Opacity = 0;

        vm = viewModel;
        BindingContext = vm;

        //vm.RoundTransitionRequested += AnimateRoundTransition;
        //vm.CorrectSymbolHit += AnimateCorrectSymbol;
        //vm.TimeoutShakeRequested += AnimateTimeoutShake;
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
        if (ScoreBoard != null)
        {
            await ScoreBoard.FadeTo(1, 500, Easing.CubicOut);
        }

        if (StackCollection != null)
        {
            await StackCollection.FadeTo(1, 400, Easing.CubicOut);
        }

        if (PlayerCollection != null)
        {
            await PlayerCollection.FadeTo(1, 400);
        }
    }

    public async Task AnimateExitAsync()
    {
        try
        {
            List<Task> tasks = [];

            if (ScoreBoard != null)
                tasks.Add(ScoreBoard.FadeTo(0, 300, Easing.CubicOut));

            if (StackCollection != null)
                tasks.Add(StackCollection.FadeTo(0, 300, Easing.CubicIn));

            if (PlayerCollection != null)
                tasks.Add(PlayerCollection.FadeTo(0, 300));

            await Task.WhenAll(tasks);
        }
        catch { }
        finally
        {
            hasAnimated = false;
        }
    }

    private async void AnimateRoundTransition()
    {
        try
        {
            // Fade out both cards
            await Task.WhenAll(
                StackCollection.FadeTo(0, 200),
                PlayerCollection.FadeTo(0, 200));

            // Small scale-down
            await Task.WhenAll(
                StackCollection.ScaleTo(0.9, 150),
                PlayerCollection.ScaleTo(0.9, 150));

            // Fade in new round
            await Task.WhenAll(
                StackCollection.FadeTo(1, 300),
                PlayerCollection.FadeTo(1, 300));

            await Task.WhenAll(
                StackCollection.ScaleTo(1, 200),
                PlayerCollection.ScaleTo(1, 200));
        }
        catch { }
    }

    private async void AnimateCorrectSymbol(EmojItSymbol symbol)
    {
        try
        {
            Layout? container = PlayerCollection
                .Children.OfType<Layout>()
                .FirstOrDefault(v => v.BindingContext == symbol);

            if (container is not null)
            {
                if (container is Layout layout && layout.Children.FirstOrDefault() is VisualElement elem)
                {
                    await PulseElement(elem);
                }
            }
        }
        catch { }
    }
    private static async Task PulseElement(VisualElement element)
    {
        try
        {
            await element.ScaleTo(1.15, 100, Easing.CubicOut);
            await element.ScaleTo(1.0, 150, Easing.CubicIn);

            if (element is Border border)
            {
                Brush? originalColor = border.Stroke;
                border.Stroke = Colors.Gold;
                await Task.Delay(200);
                border.Stroke = originalColor;
            }
        }
        catch { }
    }
    private async void AnimateTimeoutShake()
    {
        try
        {
            // Shake the player card to show timeout
            const uint duration = 50;
            for (int i = 0; i < 3; i++)
            {
                await PlayerCollection.TranslateTo(-10, 0, duration);
                await PlayerCollection.TranslateTo(10, 0, duration);
            }
            await PlayerCollection.TranslateTo(0, 0, duration);
        }
        catch { }
    }
}