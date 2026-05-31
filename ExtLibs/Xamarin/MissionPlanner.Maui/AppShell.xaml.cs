using MissionPlanner.Maui.GCSViews;

namespace MissionPlanner.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation (mirrors the legacy menu TargetTypes).
        Routing.RegisterRoute("winforms", typeof(WinFormsHostPage));
    }
}
