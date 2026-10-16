// Image.cs  –  Abstract base for Bitmap and other image types.
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;

namespace System.Drawing
{
    [Serializable]
    public abstract class Image : IDisposable, ICloneable, ISerializable
    {
        public abstract int   Width  { get; }
        public abstract int   Height { get; }
        public          Size  Size   => new Size(Width, Height);

        public virtual PixelFormat PixelFormat => PixelFormat.Format32bppArgb;
        public object?  Tag  { get; set; }
        public virtual float HorizontalResolution => 96f;
        public virtual float VerticalResolution   => 96f;
        public virtual int[]     FrameDimensionsList => Array.Empty<int>();
        public virtual PropertyItem[] PropertyItems   => Array.Empty<PropertyItem>();

        // ------------------------------------------------------------------ //

        public static Image FromStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            return new Bitmap(stream);
        }

        public static Image FromFile(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            return new Bitmap(filename);
        }

        // ------------------------------------------------------------------ //

        public abstract object Clone();
        public virtual  void   Dispose() { }
        public virtual  void   RotateFlip(RotateFlipType type) { }

        public virtual  int  GetFrameCount(FrameDimension dimension) => 1;
        public virtual  int  SelectActiveFrame(FrameDimension dimension, int frameIndex) => 0;

        public virtual Image GetThumbnailImage(int thumbWidth, int thumbHeight,
            GetThumbnailImageAbort? callback, IntPtr callbackData)
            => new Bitmap(thumbWidth, thumbHeight);

        public virtual void Save(Stream stream, ImageFormat format) { }
        public virtual void Save(string filename) { }
        public virtual void Save(string filename, ImageFormat format) { }
        public virtual void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters? encoderParams) { }
        public virtual void Save(string filename, ImageCodecInfo encoder, EncoderParameters? encoderParams) { }

        // Serialization stub
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
        protected Image(SerializationInfo info, StreamingContext context) { }
        protected Image() { }
    }
}
