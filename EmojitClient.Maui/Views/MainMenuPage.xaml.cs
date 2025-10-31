using EmojitClient.Maui.Framework.Base;
using EmojitClient.Maui.ViewModels;

namespace EmojitClient.Maui.Views;

public partial class MainMenuPage : BasePage
{
    private bool hasAnimated = false;

    public MainMenuPage(MainMenuViewModel viewModel)
    {
        InitializeComponent();

        //Opacity = 0;
        //LogoImage.Opacity = 0;
        //MainButtons.Opacity = 0;
        //FooterLabel.Opacity = 0;

        BindingContext = viewModel;
    }

    //protected override async void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    if (hasAnimated) return;

    //    try
    //    {
    //        await AnimateExitAsync();
    //    }
    //    catch { }
    //    finally
    //    {
    //        hasAnimated = true;
    //    }
    //}
    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    if (hasAnimated) return;

    //    try
    //    {
    //        await AnimateEntranceAsync();
    //    }
    //    catch { }
    //    finally
    //    {
    //        hasAnimated = true;
    //    }
    //}
    //private async Task AnimateEntranceAsync()
    //{
    //    if (LogoImage != null)
    //    {
    //        await LogoImage.FadeTo(1, 800, Easing.CubicOut);
    //    }

    //    if (MainButtons != null)
    //    {
    //        await MainButtons.FadeTo(1, 400);
    //    }

    //    if (FooterLabel != null)
    //    {
    //        await FooterLabel.FadeTo(1, 600, Easing.CubicIn);
    //    }
    //}
    //public async Task AnimateExitAsync()
    //{
    //    try
    //    {
    //        List<Task> tasks = [];

    //        if (LogoImage != null)
    //            tasks.Add(LogoImage.FadeTo(0, 300, Easing.CubicOut));

    //        if (MainButtons != null)
    //            tasks.Add(MainButtons.FadeTo(0, 300, Easing.CubicIn));

    //        if (FooterLabel != null)
    //            tasks.Add(FooterLabel.FadeTo(0, 300));

    //        await Task.WhenAll(tasks);
    //    }
    //    catch { }
    //    finally
    //    {
    //        hasAnimated = false;
    //    }
    //}
}
