using Foundation;

namespace MissionPlanner.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // TODO (Phase 3): register iOS platform services before the UI loads, e.g.:
    //   Xamarin.Test.BlueToothDevice = new MissionPlanner.Maui.iOS.BTDevice();   // CoreBluetooth
    //   Xamarin.Test.GPS             = new MissionPlanner.Maui.iOS.GPS();         // Microsoft.Maui Geolocation
    //   Xamarin.Test.SystemInfo      = new MissionPlanner.Maui.iOS.SystemInfo();
    // Port these from ExtLibs/Xamarin/Xamarin.iOS/{Main,Serial}.cs. See PHASE3-NOTES.md.
}
