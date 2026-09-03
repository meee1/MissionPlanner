// Brush.cs  –  Brush hierarchy.  Pure C#, no Unity/Skia dependency.
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Drawing
{
    public abstract class Brush : IDisposable, ICloneable
    {
        // Resolved at paint time by the rasterizer.
        public abstract Color GetColor(float x, float y, int width, int height);
        public abstract object Clone();
        public virtual void Dispose() { }
    }

    public sealed class SolidBrush : Brush
    {
        public Color Color { get; set; }
        public SolidBrush()            { }
        public SolidBrush(Color color) { Color = color; }
        public override Color GetColor(float x, float y, int w, int h) => Color;
        public override object Clone() => new SolidBrush(Color);
    }

    public sealed class TextureBrush : Brush
    {
        private readonly Bitmap _bmp;
        public WrapMode WrapMode { get; set; }
        public TextureBrush(Image image) { _bmp = image as Bitmap ?? new Bitmap(image); }
        public TextureBrush(Image image, WrapMode mode) : this(image) { WrapMode = mode; }
        public TextureBrush(Image image, RectangleF dstRect) : this(image) { }
        public TextureBrush(Image image, WrapMode mode, RectangleF dstRect) : this(image) { WrapMode = mode; }
        public override Color GetColor(float x, float y, int w, int h)
        {
            int tx = ((int)x % _bmp.Width  + _bmp.Width)  % _bmp.Width;
            int ty = ((int)y % _bmp.Height + _bmp.Height) % _bmp.Height;
            return _bmp.GetPixel(tx, ty);
        }
        public override object Clone() => new TextureBrush(_bmp);
    }

    public sealed class HatchBrush : Brush
    {
        public HatchStyle HatchStyle { get; }
        public Color ForegroundColor { get; }
        public Color BackgroundColor { get; }
        public HatchBrush(HatchStyle style, Color fg) { HatchStyle = style; ForegroundColor = fg; BackgroundColor = Color.Transparent; }
        public HatchBrush(HatchStyle style, Color fg, Color bg) { HatchStyle = style; ForegroundColor = fg; BackgroundColor = bg; }
        public override Color GetColor(float x, float y, int w, int h)
        {
            // Simple: alternate FG/BG on a 4-pixel grid
            return (((int)x + (int)y) & 3) == 0 ? ForegroundColor : BackgroundColor;
        }
        public override object Clone() => new HatchBrush(HatchStyle, ForegroundColor, BackgroundColor);
    }

    public sealed class LinearGradientBrush : Brush
    {
        private readonly Color _c1, _c2;
        private readonly PointF _pt1, _pt2;
        public ColorBlend? InterpolationColors { get; set; }
        public WrapMode WrapMode { get; set; }
        public LinearGradientBrush(PointF pt1, PointF pt2, Color c1, Color c2) { _pt1=pt1; _pt2=pt2; _c1=c1; _c2=c2; }
        public LinearGradientBrush(Point  pt1, Point  pt2, Color c1, Color c2) : this(new PointF(pt1.X,pt1.Y),new PointF(pt2.X,pt2.Y),c1,c2) { }
        public LinearGradientBrush(RectangleF rect, Color c1, Color c2, LinearGradientMode mode) { _c1=c1;_c2=c2;_pt1=rect.Location; _pt2=new PointF(rect.Right,rect.Bottom); }
        public LinearGradientBrush(Rectangle  rect, Color c1, Color c2, LinearGradientMode mode) : this(new RectangleF(rect.X,rect.Y,rect.Width,rect.Height),c1,c2,mode) { }
        public LinearGradientBrush(RectangleF rect, Color c1, Color c2, float angle) { _c1=c1;_c2=c2;_pt1=rect.Location;_pt2=new PointF(rect.Right,rect.Bottom); }
        public override Color GetColor(float x, float y, int w, int h)
        {
            float dx = _pt2.X - _pt1.X, dy = _pt2.Y - _pt1.Y;
            float len = MathF.Sqrt(dx*dx+dy*dy);
            float t   = len < 0.0001f ? 0f : MathCompat.Clamp(((x-_pt1.X)*dx+(y-_pt1.Y)*dy)/(len*len), 0f, 1f);
            return BlendColor(_c1, _c2, t);
        }
        private static Color BlendColor(Color a, Color b, float t)
        {
            int r = (int)(a.R + (b.R-a.R)*t);
            int g = (int)(a.G + (b.G-a.G)*t);
            int bl= (int)(a.B + (b.B-a.B)*t);
            int al= (int)(a.A + (b.A-a.A)*t);
            return Color.FromArgb(al,r,g,bl);
        }
        public override object Clone() => new LinearGradientBrush(_pt1,_pt2,_c1,_c2);
    }

    public sealed class PathGradientBrush : Brush
    {
        private readonly PointF[] _pts;
        public Color   CenterColor      { get; set; }
        public Color[] SurroundColors   { get; set; } = Array.Empty<Color>();
        public PointF  CenterPoint      { get; set; }
        public ColorBlend? InterpolationColors { get; set; }
        public PathGradientBrush(PointF[] pts) { _pts = pts; }
        public PathGradientBrush(Drawing2D.GraphicsPath path) { _pts = Array.Empty<PointF>(); }
        public override Color GetColor(float x, float y, int w, int h) => CenterColor;
        public override object Clone() => new PathGradientBrush(_pts) { CenterColor = CenterColor };
    }

    // ColorBlend used by LinearGradientBrush / PathGradientBrush.
    public sealed class ColorBlend
    {
        public Color[] Colors   { get; set; } = Array.Empty<Color>();
        public float[] Positions{ get; set; } = Array.Empty<float>();
        public ColorBlend() { }
        public ColorBlend(int count) { Colors = new Color[count]; Positions = new float[count]; }
    }
}
