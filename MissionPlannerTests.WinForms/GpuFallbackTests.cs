using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Drawing;
using SkiaSharp;

namespace MissionPlanner.Tests.WinForms
{
    /// <summary>
    /// Validates the CPU-fallback half of the optional GPU rendering path
    /// (MissionPlanner.Drawing.GpuContext). The GPU half needs a live GL context
    /// and cannot run headless; these tests prove that with no context registered
    /// the factory yields a working CPU raster surface, so all drawing code (and
    /// the test/CI environment) keeps working unchanged.
    /// </summary>
    [TestClass]
    public class GpuFallbackTests
    {
        [TestCleanup]
        public void Cleanup() => GpuContext.Unregister();

        [TestMethod]
        [TestCategory("WinForms")]
        public void NoContextRegistered_IsNotGpuAvailable()
        {
            GpuContext.Unregister();
            Assert.IsFalse(GpuContext.IsGpuAvailable);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void OffscreenSurface_FallsBackToCpu_AndIsDrawable()
        {
            GpuContext.Unregister();

            using var surface = GpuContext.CreateOffscreenSurface(120, 90, out bool gpu);

            Assert.IsFalse(gpu, "no GL context registered, so it must use the CPU raster path");
            Assert.IsNotNull(surface);

            // The fallback surface must be a real, drawable canvas.
            surface.Canvas.Clear(SKColors.Red);
            surface.Canvas.Flush();

            using var image = surface.Snapshot();
            using var bmp = SKBitmap.FromImage(image);
            Assert.AreEqual(120, bmp.Width);
            Assert.AreEqual(90, bmp.Height);
            var px = bmp.GetPixel(60, 45);
            Assert.AreEqual((byte)255, px.Red);
            Assert.AreEqual((byte)0, px.Green);
            Assert.AreEqual((byte)0, px.Blue);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void RenderTargetSurface_FallsBackToCpu_WhenNoContext()
        {
            GpuContext.Unregister();

            using var surface = GpuContext.CreateRenderTargetSurface(64, 48, out bool gpu);

            Assert.IsFalse(gpu);
            Assert.IsNotNull(surface);
            surface.Canvas.Clear(SKColors.Blue);
            surface.Canvas.Flush();
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void GraphicsHandleCtor_UsesFallbackSurface_AndDraws()
        {
            GpuContext.Unregister();

            // The GPU-intended Graphics(IntPtr,w,h) ctor now routes through the
            // factory; with no context it yields a usable CPU surface.
            using var g = new System.Drawing.Graphics(System.IntPtr.Zero, 100, 80);
            Assert.IsNotNull(g.Surface);
            g.Surface.Canvas.Clear(SKColors.Lime); // (0,255,0)
            g.Surface.Canvas.Flush();

            using var image = g.Surface.Snapshot();
            using var bmp = SKBitmap.FromImage(image);
            var px = bmp.GetPixel(50, 40);
            Assert.AreEqual((byte)0, px.Red);
            Assert.AreEqual((byte)255, px.Green);
            Assert.AreEqual((byte)0, px.Blue);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void SKGLControl_ConstructsHeadless_WithoutRegisteringGpu()
        {
            GpuContext.Unregister();

            using var c = new SkiaSharp.Views.Desktop.SKGLControl();
            Assert.IsInstanceOfType(c, typeof(System.Windows.Forms.Control));
            // No real GL context headless: it must not claim acceleration or
            // register a GPU context just by being constructed.
            Assert.IsFalse(c.IsHardwareAccelerated);
            Assert.IsFalse(GpuContext.IsGpuAvailable);
        }
    }
}
