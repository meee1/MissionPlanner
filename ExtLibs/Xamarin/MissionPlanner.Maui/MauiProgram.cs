using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MissionPlanner.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // SkiaSharp views (SKGLView / SKCanvasView) used by the WinForms host page.
            .UseSkiaSharp()
            // CommunityToolkit.Maui replaces the legacy Acr.UserDialogs / FilePicker plugins.
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                // TODO: drop OpenSans-Regular.ttf / OpenSans-Semibold.ttf (standard MAUI template
                // fonts) into Resources/Fonts and register them here, e.g.:
                //   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                //   fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
