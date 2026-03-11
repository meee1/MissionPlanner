// UnityRenderer.cs
// Manages the System.Drawing → Unity Texture2D rendering pipeline for a
// continuously-animating surface (e.g. the Flight Data HUD).
//
// Usage:
//   var renderer = new UnityRenderer(width, height, rawImage);
//   // each frame:
//   using var g = renderer.BeginFrame();
//   g.DrawLine(pen, 0, 0, 100, 100);   // System.Drawing calls
//   renderer.EndFrame();               // uploads to Texture2D
//
// No SkiaSharp types are used directly; they remain an internal implementation
// detail of MissionPlanner.Drawing.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MissionPlanner.Drawing.Unity;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
using UnityEngine.UI;
#endif

namespace MissionPlanner.Unity.Rendering
{
    /// <summary>
    /// Provides a <see cref="System.Drawing.Graphics"/> each frame and uploads
    /// the result to a Unity <c>Texture2D</c> / <c>RawImage</c>.
    /// </summary>
    public sealed class UnityRenderer : IDisposable
    {
        private Bitmap?   _bitmap;
        private Graphics? _graphics;
        private byte[]    _buffer = Array.Empty<byte>();
        private int       _width;
        private int       _height;
        private bool      _disposed;

#if UNITY_ENGINE_PRESENT
        private Texture2D? _texture;
        private RawImage?  _target;
#else
        private object? _texture;
        private object? _target;
#endif

        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        /// <param name="width">Logical pixel width.</param>
        /// <param name="height">Logical pixel height.</param>
        /// <param name="target">RawImage that will display the rendered frame.</param>
        public UnityRenderer(int width, int height, RawImage? target = null)
        {
            _target = target;
            Resize(width, height);
        }
#else
        public UnityRenderer(int width, int height, object? target = null)
        {
            _target = target;
            Resize(width, height);
        }
#endif

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Starts a new frame.  Returns the <see cref="Graphics"/> to draw on.
        /// The caller must NOT dispose it; call <see cref="EndFrame"/> instead.
        /// </summary>
        public Graphics BeginFrame()
        {
            if (_bitmap == null || _graphics == null)
                throw new ObjectDisposedException(nameof(UnityRenderer));

            _graphics.Clear(_bitmap.GetPixel(0, 0).IsEmpty
                ? Color.Transparent
                : Color.Transparent);
            return _graphics;
        }

        /// <summary>
        /// Finalises the current frame and uploads pixels to the Texture2D.
        /// Must be called on the Unity main thread.
        /// </summary>
        public void EndFrame()
        {
#if UNITY_ENGINE_PRESENT
            if (_bitmap == null || _texture == null) return;

            var rect    = new Rectangle(0, 0, _width, _height);
            var bmpData = _bitmap.LockBits(rect, ImageLockMode.ReadOnly,
                                           PixelFormat.Format32bppArgb);
            Marshal.Copy(bmpData.Scan0, _buffer, 0, _buffer.Length);
            _bitmap.UnlockBits(bmpData);

            _texture.LoadRawTextureData(_buffer);
            _texture.Apply(false, false);

            if (_target != null)
                _target.texture = _texture;
#endif
        }

        /// <summary>Resizes the rendering surface, recreating the bitmap.</summary>
        public void Resize(int width, int height)
        {
            if (width  <= 0) width  = 1;
            if (height <= 0) height = 1;

            if (_width == width && _height == height && _bitmap != null)
                return;

            _width  = width;
            _height = height;

            _graphics?.Dispose();
            _bitmap?.Dispose();

            _bitmap   = new Bitmap(width, height);
            _graphics = Graphics.FromImage(_bitmap);
            _buffer   = new byte[width * height * 4];

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
#endif
        }

        public int Width  => _width;
        public int Height => _height;

        // ------------------------------------------------------------------ //

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _graphics?.Dispose();
            _graphics = null;
            _bitmap?.Dispose();
            _bitmap = null;

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = null;
#endif
        }
    }
}
