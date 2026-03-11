// Imaging/BitmapData.cs  –  Locked pixel data returned by Bitmap.LockBits().
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging
{
    public sealed class BitmapData
    {
        public IntPtr Scan0  { get; internal set; }
        public int Stride    { get; internal set; }
        public int Width     { get; internal set; }
        public int Height    { get; internal set; }
        public PixelFormat PixelFormat { get; internal set; }
        public int Reserved { get; internal set; }

        // Handle to keep the managed array pinned while locked.
        internal GCHandle PinnedHandle;
    }
}
