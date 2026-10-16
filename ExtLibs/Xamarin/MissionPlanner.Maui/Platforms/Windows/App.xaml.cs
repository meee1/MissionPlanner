using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MissionPlanner.Maui.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        // Startup diagnostics: the WinUI host swallows managed startup exceptions into an opaque stowed
        // exception (0xc000027b), so log first-chance + unhandled exceptions to %LOCALAPPDATA%\mp_startup.log
        // to make early WinForms-host init failures (TLS, .resx deserialization, etc.) diagnosable.
        var log = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "mp_startup.log");
        void Write(string tag, System.Exception ex) =>
            System.IO.File.AppendAllText(log, $"[{tag}] {System.DateTime.Now:HH:mm:ss.fff} {ex}\r\n\r\n");
        System.AppDomain.CurrentDomain.FirstChanceException += (s, e) => { try { Write("FIRSTCHANCE", e.Exception); } catch { } };
        System.AppDomain.CurrentDomain.UnhandledException += (s, e) => { try { Write("UNHANDLED", e.ExceptionObject as System.Exception); } catch { } };

        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        PlatformServices.Register();
        return MauiProgram.CreateMauiApp();
    }
}
