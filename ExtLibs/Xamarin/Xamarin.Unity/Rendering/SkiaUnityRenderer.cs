// SkiaUnityRenderer.cs
// Manages the SkiaSharp → Unity Texture2D rendering pipeline for a
// continuously-animating surface (e.g. the Flight Data HUD).
//
// Usage:
//   var renderer = new SkiaUnityRenderer(width, height, rawImage);
//   // each frame:
//   using (var canvas = renderer.BeginFrame())
//   {
//       canvas.DrawRect(...);   // draw with SkiaSharp directly
//   }
//   renderer.EndFrame();       // uploads to Texture2D

using System;
using SkiaSharp;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
using UnityEngine.UI;
#endif

namespace Xamarin.Unity.Rendering
{
    /// <summary>
    /// Provides a SkiaSharp <see cref="SKCanvas"/> each frame and uploads the
    /// result to a Unity <c>Texture2D</c> / <c>RawImage</c>.
    /// </summary>
    public sealed class SkiaUnityRenderer : IDisposable
    {
        private SKSurface?  _surface;
        private byte[]      _buffer = Array.Empty<byte>();
        private int         _width;
        private int         _height;
        private bool        _disposed;

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
        public SkiaUnityRenderer(int width, int height, RawImage? target = null)
        {
            _target = target;
            Resize(width, height);
        }
#else
        public SkiaUnityRenderer(int width, int height, object? target = null)
        {
            _target = target;
            Resize(width, height);
        }
#endif

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Starts a new frame.  Returns the <see cref="SKCanvas"/> to draw on.
        /// Must be paired with <see cref="EndFrame"/>.
        /// </summary>
        public SKCanvas BeginFrame()
        {
            if (_surface == null)
                throw new ObjectDisposedException(nameof(SkiaUnityRenderer));

            _surface.Canvas.Clear(SKColors.Transparent);
            return _surface.Canvas;
        }

        /// <summary>
        /// Finalises the current frame and uploads pixels to the Texture2D.
        /// Must be called on the Unity main thread.
        /// </summary>
        public void EndFrame()
        {
#if UNITY_ENGINE_PRESENT
            if (_surface == null || _texture == null) return;

            using var pixmap = _surface.PeekPixels();
            if (pixmap == null) return;

            System.Runtime.InteropServices.Marshal.Copy(
                pixmap.GetPixels(), _buffer, 0, _buffer.Length);

            _texture.LoadRawTextureData(_buffer);
            _texture.Apply(false, false);

            if (_target != null)
                _target.texture = _texture;
#endif
        }

        /// <summary>Resizes the rendering surface.</summary>
        public void Resize(int width, int height)
        {
            if (width  <= 0) width  = 1;
            if (height <= 0) height = 1;

            if (_width == width && _height == height && _surface != null)
                return;

            _width  = width;
            _height = height;

            _surface?.Dispose();

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _buffer  = new byte[info.BytesSize];
            _surface = SKSurface.Create(info);

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

            _surface?.Dispose();
            _surface = null;

#if UNITY_ENGINE_PRESENT
            if (_texture != null)
                UnityEngine.Object.Destroy(_texture);
            _texture = null;
#endif
        }
    }
}
