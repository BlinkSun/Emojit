using EmojitClient.Maui.Framework.Services;
using EmojitClient.Maui.Views;
using System.Diagnostics;

namespace EmojitClient.Maui;

public partial class App : Application
{
    private readonly Page mainPage;
    private readonly ISoundService soundService;

    public App(MainMenuPage mainPage, ISoundService soundService)
    {
        InitializeComponent();
        this.mainPage = new NavigationPage(mainPage)
        {
            BarBackgroundColor = Colors.Transparent,
            BarTextColor = Colors.Transparent,
            Background = Colors.CornflowerBlue
        };
        NavigationPage.SetHasNavigationBar(this.mainPage, false);
        this.soundService = soundService;
        _ = InitializeAudioAsync();
    }

    private async Task InitializeAudioAsync()
    {
        try
        {
            await soundService.InitializeAsync();

            if (soundService.IsMusicEnabled)
            {
                soundService.PlayBgm("menu");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] SoundService init error: {ex.Message}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new(mainPage);

#if WINDOWS
        window.Title = "EmojIt!";

        // Dimensions typiques d’un écran mobile en mode portrait
        const int width = 400;
        const int height = 720;

        // Centrer la fenêtre à l’écran
        window.Width = width;
        window.Height = height;

        // Désactiver le redimensionnement
        window.MaximumWidth = width * 1.2;
        window.MaximumHeight = height * 1.2;
        window.MinimumWidth = width;
        window.MinimumHeight = height;
#endif

        return window;
    }

}