using System;
using SkiaSharp;

namespace MissionPlanner.Drawing
{
    /// <summary>
    /// Optional GPU acceleration for the SkiaSharp drawing surfaces used by the
    /// <see cref="System.Drawing.Graphics"/> shim.
    ///
    /// A host that owns a current GL context (e.g. an OpenTK GLControl / SKGLControl
    /// on the render thread) builds a shared <see cref="GRContext"/> and registers
    /// it here once. Surface creation then renders on the GPU. When no context is
    /// registered - headless CI, no GL driver, unit tests - it transparently falls
    /// back to CPU raster surfaces, so every drawing path works unchanged either way.
    ///
    /// Thread affinity: a <see cref="GRContext"/> and the surfaces created from it
    /// belong to the GL context/thread that created it. Register and the Create*
    /// calls must run on that render thread. This class only consumes a GRContext;
    /// creating the GL context itself is the host's responsibility (and is
    /// platform-specific), which keeps this assembly free of any windowing/GL
    /// dependency.
    /// </summary>
    public static class GpuContext
    {
        // GL_RGBA8 sized format, used to describe the framebuffer to Skia. Avoids a
        // dependency on a specific SkiaSharp color-format helper.
        private const uint GL_RGBA8 = 0x8058;

        private static GRContext _context;

        /// <summary>True once a host has registered a usable GPU context.</summary>
        public static bool IsGpuAvailable => _context != null;

        /// <summary>Register the host's shared GPU context (call once, on the render thread).</summary>
        public static void Register(GRContext context) => _context = context;

        /// <summary>Drop the registered context (drawing reverts to CPU raster).</summary>
        public static void Unregister() => _context = null;

        /// <summary>
        /// Wrap the currently-bound GL framebuffer in a GPU-backed surface, or fall
        /// back to a CPU raster surface. For the GPU path the caller presents the
        /// result itself (flush + SwapBuffers); for the CPU path the caller blits
        /// the surface's pixels as today.
        /// </summary>
        public static SKSurface CreateRenderTargetSurface(int width, int height, out bool gpu,
            uint framebuffer = 0, int samples = 0, int stencilBits = 8)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            var ctx = _context;
            if (ctx != null)
            {
                try
                {
                    var glInfo = new GRGlFramebufferInfo(framebuffer, GL_RGBA8);
                    var rt = new GRBackendRenderTarget(width, height, samples, stencilBits, glInfo);
                    var surface = SKSurface.Create(ctx, rt, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
                    if (surface != null)
                    {
                        gpu = true;
                        return surface;
                    }
                    rt.Dispose();
                }
                catch
                {
                    // GL state not usable on this thread / driver - fall back to CPU.
                }
            }

            gpu = false;
            return CreateRasterSurface(width, height);
        }

        /// <summary>
        /// An offscreen surface: GPU render-to-texture when a context is available,
        /// otherwise a CPU raster surface. Used for controls that draw into an
        /// offscreen surface before presenting.
        /// </summary>
        public static SKSurface CreateOffscreenSurface(int width, int height, out bool gpu)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            var ctx = _context;
            if (ctx != null)
            {
                try
                {
                    var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
                    var surface = SKSurface.Create(ctx, true, info);
                    if (surface != null)
                    {
                        gpu = true;
                        return surface;
                    }
                }
                catch
                {
                    // fall back to CPU
                }
            }

            gpu = false;
            return CreateRasterSurface(width, height);
        }

        private static SKSurface CreateRasterSurface(int width, int height) =>
            SKSurface.Create(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
    }
}
