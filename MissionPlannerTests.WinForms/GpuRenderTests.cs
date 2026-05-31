using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Drawing;
using SkiaSharp;

namespace MissionPlanner.Tests.WinForms
{
    /// <summary>
    /// Exercises the *GPU* half of <see cref="GpuContext"/> for real, by creating a
    /// headless software-rendered GL context (EGL surfaceless + Mesa llvmpipe) and
    /// registering it - the same thing a GL-capable host does. Validates that the
    /// factory then produces GPU-backed surfaces that render correctly, and that the
    /// wired Graphics(IntPtr,w,h) ctor uses them.
    ///
    /// Requires libEGL + a Mesa software driver (see tests/run-gpu-tests.sh and the
    /// gpu-tests CI job). When no GL context can be created the whole class is
    /// inconclusive, so ordinary headless runs are unaffected.
    /// </summary>
    [TestClass]
    [TestCategory("Gpu")]
    public class GpuRenderTests
    {
        private const string EGL = "libEGL.so.1";
        [DllImport(EGL)] static extern IntPtr eglGetDisplay(IntPtr id);
        [DllImport(EGL)] static extern bool eglInitialize(IntPtr dpy, out int major, out int minor);
        [DllImport(EGL)] static extern bool eglChooseConfig(IntPtr dpy, int[] attribs, IntPtr[] configs, int size, out int num);
        [DllImport(EGL)] static extern bool eglBindAPI(uint api);
        [DllImport(EGL)] static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr cfg, IntPtr share, int[] attribs);
        [DllImport(EGL)] static extern bool eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);
        [DllImport(EGL, CharSet = CharSet.Ansi)] static extern IntPtr eglGetProcAddress(string name);

        const int EGL_SURFACE_TYPE = 0x3033, EGL_PBUFFER_BIT = 0x0001, EGL_RENDERABLE_TYPE = 0x3040,
                  EGL_OPENGL_ES2_BIT = 0x0004, EGL_NONE = 0x3038, EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        const uint EGL_OPENGL_ES_API = 0x30A0;

        private static IntPtr _dpy, _ctx;
        private static GRContext _grContext;
        // Keep the proc-address delegate alive for the lifetime of the GRContext.
        private static GRGlGetProcedureAddressDelegate _getProc;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            try
            {
                _dpy = eglGetDisplay(IntPtr.Zero);
                if (_dpy == IntPtr.Zero || !eglInitialize(_dpy, out int _maj, out int _min))
                    return;

                int[] cfgAttribs = { EGL_SURFACE_TYPE, EGL_PBUFFER_BIT, EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT, EGL_NONE };
                var cfgs = new IntPtr[1];
                if (!eglChooseConfig(_dpy, cfgAttribs, cfgs, 1, out int n) || n < 1)
                    return;

                eglBindAPI(EGL_OPENGL_ES_API);
                int[] ctxAttribs = { EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE };
                _ctx = eglCreateContext(_dpy, cfgs[0], IntPtr.Zero, ctxAttribs);
                if (_ctx == IntPtr.Zero || !eglMakeCurrent(_dpy, IntPtr.Zero, IntPtr.Zero, _ctx))
                    return;

                _getProc = name => eglGetProcAddress(name);
                var glInterface = GRGlInterface.CreateGles(_getProc) ?? GRGlInterface.Create(_getProc);
                if (glInterface == null)
                    return;

                _grContext = GRContext.CreateGl(glInterface);
                if (_grContext != null)
                    GpuContext.Register(_grContext);
            }
            catch
            {
                _grContext = null; // no usable GL - tests go inconclusive
            }
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void Teardown()
        {
            GpuContext.Unregister();
            _grContext?.Dispose();
            _grContext = null;
        }

        [TestInitialize]
        public void RequireGpu()
        {
            if (_grContext == null)
                Assert.Inconclusive("No GL context available (libEGL + Mesa software driver required); skipping GPU test.");
            // Re-bind the context to whatever thread this test runs on.
            eglMakeCurrent(_dpy, IntPtr.Zero, IntPtr.Zero, _ctx);
        }

        [TestMethod]
        public void GpuContext_ReportsHardwareAvailable()
        {
            Assert.IsTrue(GpuContext.IsGpuAvailable);
        }

        [TestMethod]
        public void OffscreenSurface_IsGpuBacked_AndRendersCorrectly()
        {
            using var surface = GpuContext.CreateOffscreenSurface(128, 96, out bool gpu);
            Assert.IsTrue(gpu, "with a GRContext registered the surface must be GPU-backed");

            surface.Canvas.Clear(SKColors.Red);
            using (var p = new SKPaint { Color = SKColors.Lime })
                surface.Canvas.DrawRect(10, 10, 40, 30, p);
            surface.Canvas.Flush();
            _grContext.Flush();

            using var image = surface.Snapshot();
            using var bmp = SKBitmap.FromImage(image);
            var bg = bmp.GetPixel(100, 80);
            var rect = bmp.GetPixel(20, 20);
            Assert.AreEqual((byte)255, bg.Red, "background red");
            Assert.AreEqual((byte)0, bg.Green);
            Assert.AreEqual((byte)255, rect.Green, "rect lime");
            Assert.AreEqual((byte)0, rect.Red);
        }

        [TestMethod]
        public void GraphicsHandleCtor_RendersOnGpu()
        {
            // The wired Graphics(IntPtr,w,h) ctor draws into a GPU surface now.
            using var g = new System.Drawing.Graphics(IntPtr.Zero, 100, 80);
            g.Surface.Canvas.Clear(SKColors.Blue);
            g.Surface.Canvas.Flush();
            _grContext.Flush();

            using var image = g.Surface.Snapshot();
            using var bmp = SKBitmap.FromImage(image);
            var px = bmp.GetPixel(50, 40);
            Assert.AreEqual((byte)255, px.Blue);
            Assert.AreEqual((byte)0, px.Red);
        }
    }
}
