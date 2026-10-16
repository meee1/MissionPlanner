using Foundation;

namespace MissionPlanner.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        MissionPlanner.Maui.MacCatalyst.PlatformServices.Register();
        return MauiProgram.CreateMauiApp();
    }
}
