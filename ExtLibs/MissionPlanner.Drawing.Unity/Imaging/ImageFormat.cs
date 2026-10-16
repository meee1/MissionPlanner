// Imaging/ImageFormat.cs
namespace System.Drawing.Imaging
{
    public sealed class ImageFormat
    {
        public Guid Guid { get; }
        private readonly string _name;
        public ImageFormat(Guid guid) { Guid = guid; }
        public override string ToString() => _name ?? Guid.ToString();

        public static readonly ImageFormat MemoryBmp  = new ImageFormat(new Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Bmp        = new ImageFormat(new Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Emf        = new ImageFormat(new Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Wmf        = new ImageFormat(new Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Jpeg       = new ImageFormat(new Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Png        = new ImageFormat(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Gif        = new ImageFormat(new Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Tiff       = new ImageFormat(new Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Exif       = new ImageFormat(new Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"));
        public static readonly ImageFormat Icon       = new ImageFormat(new Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"));
    }

    public sealed class ImageCodecInfo
    {
        public Guid    Clsid      { get; set; }
        public Guid    FormatID   { get; set; }
        public string? CodecName  { get; set; }
        public string? MimeType   { get; set; }
        public string? FilenameExtension { get; set; }
        public static ImageCodecInfo[] GetImageEncoders() => Array.Empty<ImageCodecInfo>();
        public static ImageCodecInfo[] GetImageDecoders() => Array.Empty<ImageCodecInfo>();
    }

    public sealed class Encoder
    {
        public Guid Guid { get; }
        public Encoder(Guid guid) { Guid = guid; }
        public static readonly Encoder Quality     = new Encoder(new Guid("{1d5be4b5-fa4a-452d-9cdd-5db35105e7eb}"));
        public static readonly Encoder ColorDepth  = new Encoder(new Guid("{66087055-ad66-4c7c-9a18-38a2310b8337}"));
        public static readonly Encoder Compression = new Encoder(new Guid("{e09d739d-ccd4-44ee-8eba-3fbf8be4fc58}"));
        public static readonly Encoder SaveFlag    = new Encoder(new Guid("{292266fc-ac40-47bf-8cfc-a85b89a655de}"));
    }

    public sealed class EncoderParameter : IDisposable
    {
        public Encoder Encoder { get; }
        public long    Value   { get; }
        public EncoderParameter(Encoder encoder, long value) { Encoder = encoder; Value = value; }
        public void Dispose() { }
    }

    public sealed class EncoderParameters : IDisposable
    {
        public EncoderParameter[] Param { get; set; }
        public EncoderParameters() { Param = Array.Empty<EncoderParameter>(); }
        public EncoderParameters(int count) { Param = new EncoderParameter[count]; }
        public void Dispose() { }
    }

    public sealed class FrameDimension
    {
        public Guid Guid { get; }
        public FrameDimension(Guid guid) { Guid = guid; }
        public static readonly FrameDimension Time       = new FrameDimension(new Guid("{6aedbd6d-3fb5-418a-83a6-7f45229dc872}"));
        public static readonly FrameDimension Resolution = new FrameDimension(new Guid("{84236f7b-3bd3-428f-8dab-4ea1439ca315}"));
        public static readonly FrameDimension Page       = new FrameDimension(new Guid("{7462dc86-6180-4c7e-8e3f-ee7333a7a483}"));
    }

    public sealed class PropertyItem
    {
        public int    Id   { get; set; }
        public int    Len  { get; set; }
        public short  Type { get; set; }
        public byte[] Value { get; set; } = Array.Empty<byte>();
    }

    public sealed class ColorPalette
    {
        public Color[] Entries { get; } = Array.Empty<Color>();
        public int Flags { get; } = 0;
    }

    public sealed class ImageAttributes : IDisposable
    {
        public void SetColorMatrix(ColorMatrix newColorMatrix) { }
        public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode) { }
        public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type) { }
        public void ClearColorMatrix() { }
        public void SetWrapMode(Drawing2D.WrapMode mode) { }
        public void Dispose() { }
    }

    public sealed class ColorMatrix
    {
        private float[,] _m = new float[5, 5];
        public ColorMatrix() { for (int i = 0; i < 5; i++) _m[i, i] = 1f; }
        public float this[int row, int col] { get => _m[row, col]; set => _m[row, col] = value; }
    }

    public delegate bool GetThumbnailImageAbort();
}
