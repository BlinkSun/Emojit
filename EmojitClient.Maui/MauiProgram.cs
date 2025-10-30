using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;
using EmojitClient.Maui.Data;
using EmojitClient.Maui.Framework.Controls;
using EmojitClient.Maui.Framework.Navigation;
using EmojitClient.Maui.Framework.Services;
using EmojitClient.Maui.ViewModels;
using EmojitClient.Maui.Views;

namespace EmojitClient.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // obligatoire pour SKCanvasView
            .ConfigureMauiHandlers(handlers => handlers.AddHandler<ConfettiView, SKCanvasViewHandler>())
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Baloo2-Regular.ttf", "BalooRegular");
                fonts.AddFont("Baloo2-Bold.ttf", "BalooBold");
            });

#if DEBUG
        //builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IFileProviderService, MauiFileProviderService>();
        builder.Services.AddSingleton<ISoundService, SoundService>();
        builder.Services.AddSingleton<GameDataRepository>(provider =>
        {
            IFileProviderService fileProvider = provider.GetRequiredService<IFileProviderService>();
            DatabaseService db = new(fileProvider);
            Task.Run(async () => await db.EnsureDatabaseReadyAsync()); // 🔥 async sans bloquer
            return new GameDataRepository(db);
        });

        builder.Services.AddSingleton<NavigationManager>();

        builder.Services.AddTransient<MainMenuViewModel>();
        builder.Services.AddTransient<MainMenuPage>();

        builder.Services.AddTransient<ConfigSoloViewModel>();
        builder.Services.AddTransient<ConfigSoloPage>();

        builder.Services.AddTransient<PlaySoloViewModel>();
        builder.Services.AddTransient<PlaySoloPage>();

        builder.Services.AddTransient<StatsSoloViewModel>();
        builder.Services.AddTransient<StatsSoloPage>();

        return builder.Build();
    }
}