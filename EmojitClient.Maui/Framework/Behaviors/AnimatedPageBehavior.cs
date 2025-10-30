using System.Diagnostics;

namespace EmojitClient.Maui.Framework.Behaviors;

/// <summary>
/// Adds smooth fade-in / fade-out transitions to pages,
/// ensuring no flicker, concurrency, or black-screen issues.
/// </summary>
public partial class AnimatedPageBehavior : Behavior<ContentPage>
{
    private bool hasAnimatedIn;
    private bool isAnimating;

    protected override void OnAttachedTo(ContentPage page)
    {
        base.OnAttachedTo(page);

        //page.HandlerChanged += OnHandlerChanged;
        //page.NavigatedTo += OnNavigatedTo;
        page.Appearing += OnAppearing;
    }

    protected override void OnDetachingFrom(ContentPage page)
    {
        base.OnDetachingFrom(page);

        //page.HandlerChanged -= OnHandlerChanged;
        //page.NavigatedTo -= OnNavigatedTo;
        page.Appearing -= OnAppearing;
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is not ContentPage page) return;
        page.Opacity = 0;
    }

    private void OnNavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        if (sender is not ContentPage page) return;
        if (page.Opacity == 0) page.Opacity = 1;
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        if (sender is not ContentPage page) return;
        if (hasAnimatedIn || isAnimating) return;

        try
        {
            isAnimating = true;
            hasAnimatedIn = true;

            await page.FadeTo(1, 500, Easing.CubicOut);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ANIM] FadeIn error: {ex.Message}");
        }
        finally
        {
            isAnimating = false;
        }
    }

    public async Task AnimateExitAsync(Page page)
    {
        if (page == null) return;
        if (!hasAnimatedIn || isAnimating) return;

        try
        {
            isAnimating = true;

            await page.FadeTo(0, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ANIM] FadeOut error: {ex.Message}");
        }
        finally
        {
            hasAnimatedIn = false;
            isAnimating = false;
        }
    }
}
