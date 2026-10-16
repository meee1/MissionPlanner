using System;
using System.ComponentModel;
using System.Windows.Forms;
using MissionPlanner.Drawing;
using OpenTK;

namespace SkiaSharp.Views.Desktop
{
    /// <summary>
    /// GPU-accelerated counterpart to the (CPU raster) <see cref="SKControl"/>.
    ///
    /// It derives from OpenTK's <c>GLControl</c> (a real GL context on net472; a
    /// no-op shim on netstandard2.0), lazily builds a SkiaSharp <see cref="GRContext"/>
    /// from the current GL context, and renders directly into the GL framebuffer -
    /// no LockBits, no GDI DrawImage blit. Consumers keep the same drawing code:
    /// subscribe to <see cref="PaintSurface"/> exactly as with <see cref="SKControl"/>.
    ///
    /// If a GL context / GRContext cannot be created (headless, no driver), it
    /// degrades gracefully via <see cref="GpuContext"/>'s CPU fallback rather than
    /// throwing. Hosts that are never GL-capable should just use <see cref="SKControl"/>.
    ///
    /// Threading: all painting happens on the UI thread that owns the GL context.
    /// </summary>
    [DefaultEvent("PaintSurface")]
    public class SKGLControl : GLControl
    {
        private GRContext grContext;
        private bool gpuInitTried;

        [Category("Appearance")]
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

        /// <summary>True once a GL-backed GRContext has been created for this control.</summary>
        [Browsable(false)]
        public bool IsHardwareAccelerated => grContext != null;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = Width, height = Height;
            if (width <= 0 || height <= 0)
                return;

            try
            {
                MakeCurrent();
            }
            catch
            {
                // no usable GL context on this thread - nothing to present
                return;
            }

            EnsureGpuContext();

            // GPU-backed surface wrapping the default framebuffer, or a CPU raster
            // surface if no GRContext is available.
            using (var surface = GpuContext.CreateRenderTargetSurface(width, height, out bool gpu))
            {
                OnPaintSurface(new SKPaintSurfaceEventArgs(surface, new SKImageInfo(width, height)));
                surface.Canvas.Flush();

                if (gpu)
                {
                    grContext?.Flush();
                    SwapBuffers(); // present the GPU framebuffer directly
                }
                // The CPU-fallback path draws into an offscreen surface only; a
                // non-GL host should use SKControl, which blits to the window.
            }
        }

        private void EnsureGpuContext()
        {
            if (gpuInitTried)
                return;
            gpuInitTried = true;

            try
            {
                var glInterface = GRGlInterface.Create();
                if (glInterface == null)
                    return;

                grContext = GRContext.CreateGl(glInterface);
                if (grContext != null)
                    GpuContext.Register(grContext);
            }
            catch
            {
                // GL not available / not current - stays on the CPU fallback path.
                grContext = null;
            }
        }

        protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e) => PaintSurface?.Invoke(this, e);

        protected override void Dispose(bool disposing)
        {
            if (disposing && grContext != null)
            {
                GpuContext.Unregister();
                grContext.Dispose();
                grContext = null;
            }
            base.Dispose(disposing);
        }
    }
}
