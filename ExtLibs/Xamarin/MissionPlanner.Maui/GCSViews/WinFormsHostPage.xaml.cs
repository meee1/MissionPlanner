using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MissionPlanner.Maui.GCSViews;

/// <summary>
/// MAUI port of the legacy Xamarin <c>GCSViews/WinForms</c> page.
///
/// This is the Phase 1 scaffold: it stands up the SkiaSharp GL surface, the
/// invalidation pump and the touch plumbing that the Mono WinForms render loop
/// needs, but it does not yet drive the real Mission Planner UI.
///
/// Phase 2 will retarget the shared "Xamarin" project to net8 and replace the
/// placeholder paint/touch handlers below with the existing render loop from
/// <c>ExtLibs/Xamarin/Xamarin/GCSViews/WinForms.xaml.cs</c> (System.Windows.Forms
/// hosted on the SkiaSharp-backed System.Drawing in MissionPlanner.Drawing).
/// </summary>
public partial class WinFormsHostPage : ContentPage
{
    private IDispatcherTimer _invalidateTimer;

    public WinFormsHostPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Continuously repaint the surface (the legacy page used a render loop on the UI thread).
        _invalidateTimer = Dispatcher.CreateTimer();
        _invalidateTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0); // ~30 fps
        _invalidateTimer.Tick += (_, _) => SkCanvasView.InvalidateSurface();
        _invalidateTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _invalidateTimer?.Stop();
        _invalidateTimer = null;
    }

    private void SkCanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(0xD9, 0xD9, 0xD9));

        // TODO (Phase 2): hand this SKSurface to the Mono WinForms render loop:
        //   var g = new System.Drawing.Graphics(e.Surface);
        //   form.OnPaint(new PaintEventArgs(g, clip));
        // For now, draw a placeholder so the scaffold is visibly alive.
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = 24,
            TextAlign = SKTextAlign.Center
        };

        var info = e.BackendRenderTarget;
        canvas.DrawText("Mission Planner (MAUI scaffold)",
            info.Width / 2f, info.Height / 2f, paint);
    }

    private void SkCanvasView_Touch(object sender, SKTouchEventArgs e)
    {
        // TODO (Phase 2): translate SKTouchEventArgs into System.Windows.Forms
        // mouse/keyboard events and dispatch to the hosted form, as the legacy
        // WinForms.xaml.cs did (SkCanvasView_Touch).
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
            case SKTouchAction.Moved:
            case SKTouchAction.Released:
                e.Handled = true;
                break;
        }

        SkCanvasView.InvalidateSurface();
    }
}
