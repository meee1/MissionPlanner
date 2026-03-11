// Enums.cs  –  All System.Drawing / System.Drawing.Drawing2D / System.Drawing.Text enums.
// Pure C#, no Unity dependency.

namespace System.Drawing
{
    public enum GraphicsUnit    { World, Display, Pixel, Point, Inch, Document, Millimeter }
    public enum SmoothingMode   { Invalid = -1, Default = 0, HighSpeed, HighQuality, None, AntiAlias = 4 }
    public enum InterpolationMode { Invalid = -1, Default = 0, Low, High, Bilinear, Bicubic,
                                    NearestNeighbor, HighQualityBilinear, HighQualityBicubic }
    public enum CompositingMode    { SourceOver, SourceCopy }
    public enum CompositingQuality { Invalid = -1, Default = 0, HighSpeed, HighQuality, GammaCorrected, AssumeLinear }
    public enum PixelOffsetMode    { Invalid = -1, Default = 0, HighSpeed, HighQuality, None, Half }
    public enum FlushIntention     { Flush, Sync }
    public enum CoordinateSpace    { World, Page, Device }
    public enum CombineMode        { Replace, Intersect, Union, Xor, Exclude, Complement }
    public enum ContentAlignment
    {
        TopLeft = 0x001, TopCenter = 0x002, TopRight = 0x004,
        MiddleLeft = 0x010, MiddleCenter = 0x020, MiddleRight = 0x040,
        BottomLeft = 0x100, BottomCenter = 0x200, BottomRight = 0x400
    }
    public enum RotateFlipType
    {
        RotateNoneFlipNone = 0,
        Rotate90FlipNone   = 1,
        Rotate180FlipNone  = 2,
        Rotate270FlipNone  = 3,
        RotateNoneFlipX    = 4,
        Rotate90FlipX      = 5,
        Rotate180FlipX     = 6,
        Rotate270FlipX     = 7,
    }
    public enum StringAlignment   { Near, Center, Far }
    public enum StringTrimming    { None, Character, Word, EllipsisCharacter, EllipsisWord, EllipsisPath }
    [Flags]
    public enum StringFormatFlags
    {
        DirectionRightToLeft  = 0x0001, DirectionVertical    = 0x0002,
        FitBlackBox           = 0x0004, DisplayFormatControl = 0x0020,
        NoFontFallback        = 0x0400, MeasureTrailingSpaces= 0x0800,
        NoWrap                = 0x1000, LineLimit            = 0x2000, NoClip = 0x4000
    }
    public enum HotkeyPrefix   { None, Show, Hide }
    public enum FontStyle      { Regular = 0, Bold = 1, Italic = 2, Underline = 4, Strikeout = 8 }
    public enum ImageLayout    { None, Tile, Center, Stretch, Zoom }
    public enum ImageLockMode  { ReadOnly = 1, WriteOnly = 2, ReadWrite = 3, UserInputBuffer = 4 }
    public enum GraphicsState  { }
}

namespace System.Drawing.Drawing2D
{
    public enum DashStyle    { Solid, Dash, Dot, DashDot, DashDotDot, Custom }
    public enum LineCap      { Flat, Square, Round, Triangle, NoAnchor = 16, SquareAnchor,
                               RoundAnchor, DiamondAnchor, ArrowAnchor, Custom = 255, AnchorMask = 240 }
    public enum LineJoin     { Miter, Bevel, Round, MiterClipped }
    public enum PenAlignment { Center, Inset, Outset, Left, Right }
    public enum FillMode     { Alternate, Winding }
    public enum PathPointType
    {
        Start = 0, Line = 1, Bezier3 = 3, Bezier = 3,
        PathTypeMask = 7, DashMode = 16, PathMarker = 32, CloseSubpath = 128
    }
    public enum MatrixOrder        { Prepend, Append }
    public enum WarpMode           { Perspective, Bilinear }
    public enum LinearGradientMode { Horizontal, Vertical, ForwardDiagonal, BackwardDiagonal }
    public enum WrapMode           { Tile, TileFlipX, TileFlipY, TileFlipXY, Clamp }
    public enum HatchStyle
    {
        Horizontal = 0, Vertical, ForwardDiagonal, BackwardDiagonal, Cross, DiagonalCross,
        Percent05, Percent10, Percent20, Percent25, Percent30, Percent40, Percent50,
        Min = Horizontal, Max = SolidDiamond,
        LargeGrid = Cross, SolidDiamond = 50
    }
    public enum PenType  { SolidColor, HatchFill, TextureFill, PathGradient, LinearGradient }
    public enum BrushType{ SolidColor, HatchFill, TextureFill, PathGradient, LinearGradient }
}

namespace System.Drawing.Imaging
{
    public enum PixelFormat
    {
        Undefined = 0,
        Format1bppIndexed  = 0x00030101,
        Format4bppIndexed  = 0x00030402,
        Format8bppIndexed  = 0x00030803,
        Format16bppGrayScale = 0x00101004,
        Format16bppRgb555  = 0x00021005,
        Format16bppRgb565  = 0x00021006,
        Format16bppArgb1555= 0x00061007,
        Format24bppRgb     = 0x00021808,
        Format32bppRgb     = 0x00022009,
        Format32bppArgb    = 0x0026200A,
        Format32bppPArgb   = 0x000E200B,
        Format48bppRgb     = 0x0010300C,
        Format64bppArgb    = 0x0034400D,
        Format64bppPArgb   = 0x001C400E,
        Max = 15, Indexed = 0x00010000, Gdi = 0x00020000,
        Alpha = 0x00040000, PAlpha = 0x00080000,
        Extended = 0x00100000, Canonical = 0x00200000,
        DontCare = 0
    }
    public enum ImageLockMode   { ReadOnly = 1, WriteOnly = 2, ReadWrite = 3, UserInputBuffer = 4 }
    public enum MetafileFrameUnit { Pixel = 2, Point, Inch, Document, Millimeter, GdiCompatible }
    public enum EmfType         { EmfOnly = 3, EmfPlusDual, EmfPlusOnly }
    public enum ColorAdjustType  { Default, Bitmap, Brush, Pen, Text, Count, Any }
    public enum ColorMatrixFlag  { Default = 0, SkipGrays = 1, AltGray = 2 }
}

namespace System.Drawing.Text
{
    public enum TextRenderingHint
    {
        SystemDefault, SingleBitPerPixelGridFit, SingleBitPerPixel,
        AntiAliasGridFit, AntiAlias, ClearTypeGridFit
    }
    public enum GenericFontFamilies { Serif, SansSerif, Monospace }
}
