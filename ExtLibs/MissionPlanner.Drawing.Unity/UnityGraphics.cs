// UnityGraphics.cs
// Bridge between System.Drawing.Graphics and Unity's Texture2D system.
//
// Rendering pipeline:
//   1. UnityGraphicsSurface creates a System.Drawing.Bitmap as the backing store.
//   2. CreateGraphics() returns Graphics.FromImage(bitmap) — the control paints
//      into the bitmap's pixel memory via the standard WinForms paint path.
//   3. FlushToTexture() reads the bitmap pixels via LockBits and uploads them
//      to a Unity Texture2D.
//
// No SkiaSharp types are used directly in this file; they are internal to
// MissionPlanner.Drawing and hidden behind the System.Drawing API surface.
//
// This file compiles with or without UnityEngine present so CI builds succeed
// without a Unity installation.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Drawing.Unity
{
    /// <summary>
    /// Owns the <see cref="Bitmap"/> that WinForms controls paint into and can
    /// upload the result to a Unity <c>Texture2D</c> each frame.
    /// </summary>
    public sealed class UnityGraphicsSurface : IDisposable
    {
        private Bitmap? _bitmap;
        private byte[]  _pixelBuffer = Array.Empty<byte>();
        private bool    _dirty;
        private bool    _disposed;

#if UNITY_ENGINE_PRESENT
        private Texture2D? _texture;
#else
        private object? _texture;
#endif

        public int Width  { get; private set; }
        public int Height { get; private set; }

        /// <summary>Resize (or initially create) the backing bitmap.</summary>
        public void Resize(int width, int height)
        {
            if (width  <= 0) width  = 1;
            if (height <= 0) height = 1;

            if (Width == width && Height == height && _bitmap != null)
                return;

            Width  = width;
            Height = height;

            _bitmap?.Dispose();
            _bitmap      = new Bitmap(width, height);
            _pixelBuffer = new byte[width * height * 4];   // BGRA32
            _dirty       = true;

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            // BGRA32 matches the Bgra8888 pixel format used by MissionPlanner.Drawing.
            _texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
#endif
        }

        /// <summary>
        /// Returns a <see cref="Graphics"/> backed by the internal bitmap.
        /// Drawing into this Graphics updates the bitmap pixel memory directly.
        /// </summary>
        public Graphics CreateGraphics()
        {
            if (_bitmap == null)
                Resize(1, 1);

            _dirty = true;
            return Graphics.FromImage(_bitmap!);
        }

        /// <summary>
        /// Copies pixels from the bitmap into the Unity Texture2D.
        /// Must be called from the Unity main thread.
        /// </summary>
        public void FlushToTexture()
        {
#if UNITY_ENGINE_PRESENT
            if (!_dirty || _bitmap == null || _texture == null)
                return;

            var rect    = new Rectangle(0, 0, Width, Height);
            var bmpData = _bitmap.LockBits(rect, ImageLockMode.ReadOnly,
                                           PixelFormat.Format32bppArgb);
            Marshal.Copy(bmpData.Scan0, _pixelBuffer, 0, _pixelBuffer.Length);
            _bitmap.UnlockBits(bmpData);

            _texture.LoadRawTextureData(_pixelBuffer);
            _texture.Apply(false, false);

            _dirty = false;
#endif
        }

        /// <summary>
        /// Directly uploads raw BGRA32 pixel data to the Texture2D, bypassing the
        /// internal bitmap.  Used when the caller renders into a standard
        /// System.Drawing.Bitmap and needs to push the result to Unity.
        /// </summary>
        public void UploadPixels(byte[] bgra32Data)
        {
#if UNITY_ENGINE_PRESENT
            if (_texture == null || bgra32Data == null) return;
            _texture.LoadRawTextureData(bgra32Data);
            _texture.Apply(false, false);
            _dirty = false;
#endif
        }

        /// <summary>
        /// Returns the Unity Texture2D, or <c>null</c> when UnityEngine is not present.
        /// Assign this to a <c>UnityEngine.UI.RawImage.texture</c> field.
        /// </summary>
#if UNITY_ENGINE_PRESENT
        public Texture2D? Texture => _texture;
#else
        public object? Texture => _texture;
#endif

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _bitmap?.Dispose();
            _bitmap = null;

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = null;
#endif
        }
    }

    /// <summary>
    /// Creates a temporary <see cref="Graphics"/> for one-shot painting into a
    /// fresh <see cref="Bitmap"/> of the requested size.
    /// </summary>
    public static class UnityGraphicsFactory
    {
        /// <summary>
        /// Returns a <see cref="Graphics"/> and its backing <see cref="Bitmap"/>.
        /// The caller is responsible for disposing both.
        /// </summary>
        public static (Graphics graphics, Bitmap bitmap) Create(int width, int height)
        {
            if (width  <= 0) width  = 1;
            if (height <= 0) height = 1;

            var bmp = new Bitmap(width, height);
            return (Graphics.FromImage(bmp), bmp);
        }
    }
}
