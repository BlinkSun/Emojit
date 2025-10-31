using EmojitClient.Maui.Framework.Behaviors;
using EmojitClient.Maui.Framework.Controls;

namespace EmojitClient.Maui.Framework.Base;

public partial class BasePage : ContentPage
{
    private EmojiBackgroundView? emojiBg;
    private RadiantRaysView? radiantRays;
    private ConfettiView? confettiView;
    private Grid? rootGrid;

    public BasePage()
    {
        Behaviors.Add(new AnimatedPageBehavior());
    }

    public static readonly BindableProperty IsEmojiBackgroundEnabledProperty =
        BindableProperty.Create(
            nameof(IsEmojiBackgroundEnabled),
            typeof(bool),
            typeof(BasePage),
            true,
            propertyChanged: (b, o, n) => ((BasePage)b).UpdateBackgroundLayers());

    public bool IsEmojiBackgroundEnabled
    {
        get => (bool)GetValue(IsEmojiBackgroundEnabledProperty);
        set => SetValue(IsEmojiBackgroundEnabledProperty, value);
    }

    public static readonly BindableProperty IsRadiantRaysEnabledProperty =
        BindableProperty.Create(
            nameof(IsRadiantRaysEnabled),
            typeof(bool),
            typeof(BasePage),
            false,
            propertyChanged: (b, o, n) => ((BasePage)b).UpdateBackgroundLayers());

    public bool IsRadiantRaysEnabled
    {
        get => (bool)GetValue(IsRadiantRaysEnabledProperty);
        set => SetValue(IsRadiantRaysEnabledProperty, value);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        UpdateBackgroundLayers();
    }

    private void UpdateBackgroundLayers()
    {
        if (Content == null)
            return;

        if (rootGrid == null)
        {
            rootGrid = [];
            View oldContent = Content;
            Content = rootGrid;

            rootGrid.Children.Add(oldContent);
        }

        RemoveIfExists(emojiBg);
        RemoveIfExists(radiantRays);
        RemoveIfExists(confettiView);

        if (IsEmojiBackgroundEnabled)
        {
            emojiBg ??= new EmojiBackgroundView
            {
                //BackgroundColor = Colors.CornflowerBlue,
                //EmojiCount = 30,
                //EmojiOpacity = 1f,
                //EmojiSpeed = 60,
                //MaxEmojiSize = 128,
                //MinEmojiSize = 32,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            rootGrid.Children.Add(emojiBg);
        }

        if (IsRadiantRaysEnabled)
        {
            radiantRays ??= new RadiantRaysView
            {
                Margin = new Thickness(0, 0, 0, -45),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Opacity = 0.3,
                RayCount = 20,
                RayLengthFactor = 1.5f,
                RayThickness = 80,
                RotationSpeed = 30
            };
            rootGrid.Children.Add(radiantRays);
        }

        confettiView ??= new ConfettiView
        {
            BackgroundColor = Colors.Transparent,
            ExplosionForce = 15,
            Gravity = 0.25f,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ParticlesCount = 600
        };
        rootGrid.Children.Add(confettiView);
    }

    private void RemoveIfExists(View? view)
    {
        if (view != null && rootGrid != null && rootGrid.Children.Contains(view))
            rootGrid.Children.Remove(view);
    }
}