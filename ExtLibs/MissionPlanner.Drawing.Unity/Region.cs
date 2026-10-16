// Region.cs  –  Clipping region.  Pure C#.
using System.Drawing.Drawing2D;

namespace System.Drawing
{
    public sealed class Region : IDisposable
    {
        private RectangleF _bounds;

        public Region() { _bounds = new RectangleF(float.MinValue/2, float.MinValue/2, float.MaxValue, float.MaxValue); }
        public Region(Rectangle rect)    { _bounds = rect; }
        public Region(RectangleF rect)   { _bounds = rect; }
        public Region(GraphicsPath path) { _bounds = path.GetBounds(); }

        public RectangleF GetBounds(Graphics g) => _bounds;
        public RectangleF Bounds => _bounds;

        public bool IsVisible(int x, int y)              => _bounds.Contains(x, y);
        public bool IsVisible(float x, float y)          => _bounds.Contains(x, y);
        public bool IsVisible(Point pt)                  => _bounds.Contains(pt.X, pt.Y);
        public bool IsVisible(PointF pt)                 => _bounds.Contains(pt.X, pt.Y);
        public bool IsVisible(Rectangle rect)            => _bounds.IntersectsWith(rect);
        public bool IsVisible(RectangleF rect)           => _bounds.IntersectsWith(rect);
        public bool IsInfinite(Graphics g)               => _bounds.Width >= float.MaxValue / 2;
        public bool IsEmpty(Graphics g)                  => _bounds.IsEmpty;

        public void Intersect(RectangleF rect)           { _bounds = RectangleF.Intersect(_bounds, rect); }
        public void Intersect(Rectangle  rect)           => Intersect((RectangleF)rect);
        public void Intersect(Region     region)         => Intersect(region._bounds);
        public void Union(RectangleF rect)               { /* expand bounds */ _bounds = RectangleF.Union(_bounds, rect); }
        public void Union(Rectangle  rect)               => Union((RectangleF)rect);
        public void Exclude(RectangleF rect)             { /* simplified: no-op */ }
        public void Xor(RectangleF rect)                 { /* simplified: no-op */ }
        public void MakeEmpty()                          { _bounds = RectangleF.Empty; }
        public void MakeInfinite()                       { _bounds = new RectangleF(float.MinValue/2,float.MinValue/2,float.MaxValue,float.MaxValue); }

        public Region Clone() => new Region(_bounds);
        public void   Dispose() { }
    }
}
