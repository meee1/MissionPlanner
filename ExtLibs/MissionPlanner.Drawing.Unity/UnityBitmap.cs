// UnityBitmap.cs
// Utility helpers that convert between System.Drawing.Bitmap (SkiaSharp-backed)
// and Unity Texture2D.
//
// Compiles with or without UnityEngine present.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Drawing.Unity
{
    /// <summary>
    /// Converts between <see cref="System.Drawing.Bitmap"/> and Unity
    /// <c>Texture2D</c>/<c>Sprite</c>.
    /// </summary>
    public static class UnityBitmapConverter
    {
        // ------------------------------------------------------------------ //
        //  Bitmap  →  Texture2D                                               //
        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        /// <summary>
        /// Uploads a <see cref="Bitmap"/>'s pixels to a new Unity
        /// <c>Texture2D</c> using BGRA32 format.
        /// </summary>
        public static Texture2D ToTexture2D(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            int w = bitmap.Width;
            int h = bitmap.Height;

            var tex = new Texture2D(w, h, TextureFormat.BGRA32, false);

            var skBitmap = bitmap.nativeSkBitmap;
            byte[] raw = new byte[w * h * 4];
            System.Runtime.InteropServices.Marshal.Copy(skBitmap.GetPixels(), raw, 0, raw.Length);

            tex.LoadRawTextureData(raw);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Creates a Unity <c>Sprite</c> from a <see cref="Bitmap"/>.
        /// </summary>
        public static Sprite ToSprite(Bitmap bitmap)
        {
            var tex = ToTexture2D(bitmap);
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }

        // ------------------------------------------------------------------ //
        //  Texture2D  →  Bitmap                                               //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Downloads a Unity <c>Texture2D</c>'s pixels into a new
        /// <see cref="Bitmap"/>.
        /// </summary>
        public static Bitmap ToBitmap(Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            // GetRawTextureData works for readable textures.
            byte[] raw = texture.GetRawTextureData();
            int w = texture.width;
            int h = texture.height;

            // Unity stores texture rows bottom-up; flip vertically.
            byte[] flipped = FlipVertical(raw, w, h, 4);

            var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
            var skBmp = new SKBitmap();
            unsafe
            {
                fixed (byte* p = flipped)
                {
                    skBmp.InstallPixels(info, (IntPtr)p);
                }
            }

            // Deep-copy so the fixed buffer can be released.
            var result = new Bitmap(w, h);
            result.nativeSkBitmap = skBmp.Copy();
            return result;
        }
#endif

        // ------------------------------------------------------------------ //
        //  Helpers                                                             //
        // ------------------------------------------------------------------ //

        private static byte[] FlipVertical(byte[] src, int width, int height, int bpp)
        {
            int stride = width * bpp;
            byte[] dst = new byte[src.Length];
            for (int row = 0; row < height; row++)
            {
                int srcRow = (height - 1 - row) * stride;
                int dstRow = row * stride;
                Buffer.BlockCopy(src, srcRow, dst, dstRow, stride);
            }
            return dst;
        }
    }
}
