// Icon.cs  –  System.Drawing.Icon wrapper.  Backed by a Bitmap internally.
using System.IO;

namespace System.Drawing
{
    public sealed class Icon : IDisposable, ICloneable
    {
        private Bitmap _bmp;

        public int    Width  => _bmp.Width;
        public int    Height => _bmp.Height;
        public Size   Size   => _bmp.Size;

        public Icon(Bitmap bmp)         { _bmp = bmp; }
        public Icon(Stream stream)      { _bmp = new Bitmap(stream); }
        public Icon(string filename)    { _bmp = new Bitmap(filename); }
        public Icon(Icon original, Size size) { _bmp = new Bitmap(original._bmp, size); }
        public Icon(Icon original, int w, int h) { _bmp = new Bitmap(original._bmp, w, h); }

        public static Icon? FromHandle(IntPtr handle) => null;
        public static Icon  ExtractAssociatedIcon(string path) => new Icon(new Bitmap(32, 32));

        public Bitmap ToBitmap() => (Bitmap)_bmp.Clone();
        public IntPtr Handle     => IntPtr.Zero;

        public object Clone() => new Icon((Bitmap)_bmp.Clone());
        public void   Dispose() { }
    }
}
