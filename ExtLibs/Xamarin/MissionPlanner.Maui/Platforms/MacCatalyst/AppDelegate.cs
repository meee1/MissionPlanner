using Foundation;

namespace MissionPlanner.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // TODO (Phase 3): register Mac Catalyst platform services before the UI loads, e.g.:
    //   Xamarin.Test.BlueToothDevice = new MissionPlanner.Maui.MacCatalyst.BTDevice();
    //   Xamarin.Test.UsbDevices      = new MissionPlanner.Maui.MacCatalyst.USBDevices();
    //   Xamarin.Test.Speech          = new MissionPlanner.Maui.MacCatalyst.Speech();
    //   Xamarin.Test.SystemInfo      = new MissionPlanner.Maui.MacCatalyst.SystemInfo();
    // The legacy impls in ExtLibs/Xamarin/Xamarin.MacOS/AppDelegate.cs are AppKit-based and need
    // reworking for Catalyst (UIKit). GPS.cs / SystemInfo.cs are portable. See PHASE3-NOTES.md.
}
