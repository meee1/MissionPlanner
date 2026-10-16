using Microsoft.Maui.Controls;

namespace MissionPlanner.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
