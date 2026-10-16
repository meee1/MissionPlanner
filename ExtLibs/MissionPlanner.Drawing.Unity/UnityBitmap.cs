// UnityBitmap.cs
// Utility helpers that convert between System.Drawing.Bitmap and Unity Texture2D.
//
// Uses only System.Drawing.Imaging types (LockBits / BitmapData) to read and
// write pixel data — no SkiaSharp types are referenced directly.
//
// Compiles with or without UnityEngine present.

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
    /// Converts between <see cref="System.Drawing.Bitmap"/> and Unity
    /// <c>Texture2D</c> / <c>Sprite</c>.
    /// </summary>
    public static class UnityBitmapConverter
    {
        // ------------------------------------------------------------------ //
        //  Bitmap  →  Texture2D                                               //
        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        /// <summary>
        /// Uploads a <see cref="Bitmap"/>'s pixels to a new Unity
        /// <c>Texture2D</c> (BGRA32 format).
        /// </summary>
        public static Texture2D ToTexture2D(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            int w = bitmap.Width;
            int h = bitmap.Height;

            var raw = ReadRawPixels(bitmap);

            var tex = new Texture2D(w, h, TextureFormat.BGRA32, false);
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
        /// The texture must be readable (Read/Write enabled in import settings).
        /// </summary>
        public static Bitmap ToBitmap(Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            // GetRawTextureData returns rows in bottom-up order on most Unity
            // platforms; flip so WinForms (top-down) reads correctly.
            byte[] raw     = texture.GetRawTextureData();
            int    w       = texture.width;
            int    h       = texture.height;
            byte[] flipped = FlipVertical(raw, w, h, bytesPerPixel: 4);

            // Create a Bitmap and write the flipped pixels in via LockBits.
            var bmp     = new Bitmap(w, h);
            var rect    = new Rectangle(0, 0, w, h);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly,
                                       PixelFormat.Format32bppArgb);
            Marshal.Copy(flipped, 0, bmpData.Scan0, flipped.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
#endif

        // ------------------------------------------------------------------ //
        //  Internal helpers                                                    //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Reads the raw BGRA32 pixel bytes from a <see cref="Bitmap"/> via
        /// <c>LockBits</c> — no SkiaSharp types required.
        /// </summary>
        internal static byte[] ReadRawPixels(Bitmap bitmap)
        {
            var rect    = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,
                                          PixelFormat.Format32bppArgb);
            var buf = new byte[bmpData.Stride * bitmap.Height];
            Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
            bitmap.UnlockBits(bmpData);
            return buf;
        }

        private static byte[] FlipVertical(byte[] src, int width, int height, int bytesPerPixel)
        {
            int stride = width * bytesPerPixel;
            var dst    = new byte[src.Length];
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
