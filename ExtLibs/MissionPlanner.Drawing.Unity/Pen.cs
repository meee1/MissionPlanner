// Pen.cs  –  Drawing pen.  Pure C#, no Unity/Skia dependency.
using System.Drawing.Drawing2D;

namespace System.Drawing
{
    public sealed class Pen : IDisposable, ICloneable
    {
        public Color     Color     { get; set; }
        public float     Width     { get; set; } = 1f;
        public Brush     Brush     { get; set; }
        public DashStyle DashStyle { get; set; } = DashStyle.Solid;
        public float[]   DashPattern { get; set; } = Array.Empty<float>();
        public float     DashOffset  { get; set; }
        public LineJoin  LineJoin  { get; set; } = LineJoin.Miter;
        public LineCap   StartCap  { get; set; } = LineCap.Flat;
        public LineCap   EndCap    { get; set; } = LineCap.Flat;
        public PenAlignment Alignment { get; set; } = PenAlignment.Center;
        public float     MiterLimit   { get; set; } = 10f;
        public CustomLineCap? CustomStartCap { get; set; }
        public CustomLineCap? CustomEndCap   { get; set; }

        public Pen(Color color)               { Color = color;  Brush = new SolidBrush(color); }
        public Pen(Color color, float width)  { Color = color;  Width = width; Brush = new SolidBrush(color); }
        public Pen(Brush brush)               { Brush = brush;  Color = (brush as SolidBrush)?.Color ?? Color.Black; }
        public Pen(Brush brush, float width)  { Brush = brush;  Width = width; Color = (brush as SolidBrush)?.Color ?? Color.Black; }

        public Pen Clone() => (Pen)((ICloneable)this).Clone();
        object ICloneable.Clone() => new Pen(Color, Width) { DashStyle = DashStyle, DashPattern = (float[])DashPattern.Clone(), LineJoin = LineJoin, StartCap = StartCap, EndCap = EndCap };
        public void Dispose() { }
    }

    // Stub – no rendering effect in the software rasterizer.
    public sealed class CustomLineCap : IDisposable
    {
        public LineCap BaseCap  { get; set; } = LineCap.Flat;
        public float   BaseInset{ get; set; }
        public void Dispose() { }
    }
}
