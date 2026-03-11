// UnityGraphics.cs
// A thin bridge between System.Drawing.Graphics (SkiaSharp-backed) and Unity's
// Texture2D / RenderTexture system.
//
// Usage pattern (mirrors how Xamarin.Android's MySKCanvasView works):
//
//   1. UnityGraphicsSurface is created per control / canvas object.
//   2. OnPaint / OnDraw raises a PaintEventArgs whose Graphics wraps a SkiaSharp
//      SKSurface that renders into an internal byte buffer.
//   3. After each paint cycle, FlushToTexture() uploads the buffer pixels to a
//      UnityEngine.Texture2D that is assigned to a Unity UI RawImage component.
//
// This file compiles both with and without UnityEngine present so that the
// standard dotnet build pipeline (used for CI / IDE intellisense) succeeds.

using System;
using System.Drawing;         // from MissionPlanner.Drawing (SkiaSharp wrapper)
using SkiaSharp;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Drawing.Unity
{
    /// <summary>
    /// Owns the SkiaSharp surface that WinForms controls paint into, and can
    /// upload the result to a Unity <c>Texture2D</c> each frame.
    /// </summary>
    public sealed class UnityGraphicsSurface : IDisposable
    {
        private SKSurface?  _surface;
        private SKImageInfo _info;
        private byte[]      _pixelBuffer = Array.Empty<byte>();
        private bool        _dirty;

        // These are typed as 'object' so the file compiles without UnityEngine.
#if UNITY_ENGINE_PRESENT
        private Texture2D? _texture;
#else
        private object? _texture;
#endif

        public int Width  { get; private set; }
        public int Height { get; private set; }

        /// <summary>Resize (or initially create) the surface.</summary>
        public void Resize(int width, int height)
        {
            if (width <= 0)  width  = 1;
            if (height <= 0) height = 1;

            if (Width == width && Height == height && _surface != null)
                return;

            Width  = width;
            Height = height;

            _surface?.Dispose();

            // BGRA8888 matches Unity's TextureFormat.BGRA32 / ARGB32.
            _info        = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _pixelBuffer = new byte[_info.BytesSize];
            _surface     = SKSurface.Create(_info);
            _dirty       = true;

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
#endif
        }

        /// <summary>
        /// Returns a <see cref="System.Drawing.Graphics"/> instance that paints
        /// into the internal SkiaSharp surface.
        /// </summary>
        public Graphics CreateGraphics()
        {
            if (_surface == null)
                Resize(1, 1);

            _dirty = true;
            return new Graphics(_surface!);
        }

        /// <summary>
        /// Copies pixels from the SkiaSharp surface into the Unity Texture2D.
        /// Call this once per frame after all painting is done.
        ///
        /// Must be called from the Unity main thread.
        /// </summary>
        public void FlushToTexture()
        {
#if UNITY_ENGINE_PRESENT
            if (!_dirty || _surface == null || _texture == null)
                return;

            // Read pixels from the SkiaSharp surface into the managed buffer.
            using var pixmap = _surface.PeekPixels();
            if (pixmap == null) return;

            System.Runtime.InteropServices.Marshal.Copy(
                pixmap.GetPixels(), _pixelBuffer, 0, _pixelBuffer.Length);

            // Upload to the GPU texture.
            _texture.LoadRawTextureData(_pixelBuffer);
            _texture.Apply(false, false);

            _dirty = false;
#endif
        }

        /// <summary>
        /// Returns the Unity Texture2D (null when UnityEngine is not present).
        /// Assign this to a <c>UnityEngine.UI.RawImage.texture</c> field.
        /// </summary>
#if UNITY_ENGINE_PRESENT
        public Texture2D? Texture => _texture;
#else
        public object? Texture => _texture;
#endif

        public void Dispose()
        {
            _surface?.Dispose();
            _surface = null;
#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = null;
#endif
        }
    }

    /// <summary>
    /// Static factory used by the Unity form host to obtain a
    /// <see cref="Graphics"/> for any given control surface.
    /// </summary>
    public static class UnityGraphicsFactory
    {
        /// <summary>
        /// Creates a <see cref="Graphics"/> that renders into a freshly-allocated
        /// SkiaSharp surface of the requested size.
        /// The caller is responsible for disposing the returned object.
        /// </summary>
        public static Graphics Create(int width, int height)
        {
            if (width  <= 0) width  = 1;
            if (height <= 0) height = 1;

            var info    = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var surface = SKSurface.Create(info);
            return new Graphics(surface);
        }
    }
}
