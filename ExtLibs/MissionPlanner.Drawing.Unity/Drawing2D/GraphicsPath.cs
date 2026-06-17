// Drawing2D/GraphicsPath.cs  –  Vector path for complex shape drawing.
// Pure C#, no Unity/Skia dependency.

using System.Collections.Generic;

namespace System.Drawing.Drawing2D
{
    public sealed class PathData
    {
        public PointF[] Points { get; set; } = Array.Empty<PointF>();
        public byte[]   Types  { get; set; } = Array.Empty<byte>();
    }

    public sealed class GraphicsPath : IDisposable, ICloneable
    {
        private readonly List<PointF> _points = new();
        private readonly List<byte>   _types  = new();

        public FillMode FillMode { get; set; }
        public PathData PathData => new PathData { Points = _points.ToArray(), Types = _types.ToArray() };
        public PointF[] PathPoints => _points.ToArray();
        public byte[]   PathTypes  => _types.ToArray();
        public int      PointCount => _points.Count;

        public GraphicsPath() { }
        public GraphicsPath(FillMode fillMode) { FillMode = fillMode; }
        public GraphicsPath(PointF[] pts, byte[] types) { _points.AddRange(pts); _types.AddRange(types); }
        public GraphicsPath(Point[]  pts, byte[] types) { foreach (var p in pts) _points.Add(new PointF(p.X,p.Y)); _types.AddRange(types); }
        public GraphicsPath(PointF[] pts, byte[] types, FillMode fm) : this(pts,types) { FillMode = fm; }

        // ------------------------------------------------------------------ //
        //  Add primitives                                                      //
        // ------------------------------------------------------------------ //

        public void StartFigure() { /* next point will be a Start */ }
        public void CloseFigure()
        {
            if (_types.Count > 0)
                _types[_types.Count-1] |= (byte)PathPointType.CloseSubpath;
        }
        public void CloseAllFigures()
        {
            for (int i = _types.Count-1; i >= 0; i--)
            {
                if ((_types[i] & (byte)PathPointType.PathTypeMask) != (byte)PathPointType.Start)
                { _types[i] |= (byte)PathPointType.CloseSubpath; break; }
            }
        }

        public void AddLine(float x1,float y1,float x2,float y2)
        {
            AddPoint(x1,y1, PathPointType.Start); AddPoint(x2,y2, PathPointType.Line);
        }
        public void AddLine(PointF p1, PointF p2) => AddLine(p1.X,p1.Y,p2.X,p2.Y);
        public void AddLine(int x1,int y1,int x2,int y2) => AddLine((float)x1,(float)y1,(float)x2,(float)y2);
        public void AddLine(Point p1, Point p2) => AddLine(p1.X,p1.Y,p2.X,p2.Y);

        public void AddLines(PointF[] pts) { if (pts.Length==0) return; AddPoint(pts[0].X,pts[0].Y,PathPointType.Start); for(int i=1;i<pts.Length;i++) AddPoint(pts[i].X,pts[i].Y,PathPointType.Line); }
        public void AddLines(Point[]  pts) { if (pts.Length==0) return; AddPoint(pts[0].X,pts[0].Y,PathPointType.Start); for(int i=1;i<pts.Length;i++) AddPoint(pts[i].X,pts[i].Y,PathPointType.Line); }

        public void AddRectangle(RectangleF r)
        {
            AddLine(r.Left,r.Top,r.Right,r.Top);
            AddLine(r.Right,r.Top,r.Right,r.Bottom);
            AddLine(r.Right,r.Bottom,r.Left,r.Bottom);
            AddLine(r.Left,r.Bottom,r.Left,r.Top);
            CloseFigure();
        }
        public void AddRectangle(Rectangle r) => AddRectangle(new RectangleF(r.X,r.Y,r.Width,r.Height));
        public void AddRectangles(RectangleF[] rects) { foreach(var r in rects) AddRectangle(r); }
        public void AddRectangles(Rectangle[]  rects) { foreach(var r in rects) AddRectangle(r); }

        public void AddEllipse(float x,float y,float w,float h) => AddEllipse(new RectangleF(x,y,w,h));
        public void AddEllipse(RectangleF rect)
        {
            AppendArcPoints(rect, 0f, 360f);
            CloseFigure();
        }
        public void AddEllipse(Rectangle r) => AddEllipse(new RectangleF(r.X,r.Y,r.Width,r.Height));
        public void AddEllipse(int x,int y,int w,int h) => AddEllipse((float)x,(float)y,(float)w,(float)h);

        public void AddArc(float x,float y,float w,float h,float start,float sweep) => AppendArcPoints(new RectangleF(x,y,w,h),start,sweep);
        public void AddArc(RectangleF rect,float start,float sweep) => AppendArcPoints(rect,start,sweep);
        public void AddArc(Rectangle  rect,float start,float sweep) => AppendArcPoints(new RectangleF(rect.X,rect.Y,rect.Width,rect.Height),start,sweep);
        public void AddArc(int x,int y,int w,int h,int start,int sweep) => AddArc((float)x,(float)y,(float)w,(float)h,(float)start,(float)sweep);

        public void AddPie(float x,float y,float w,float h,float start,float sweep) { AddLine(x+w/2,y+h/2,0,0); AddArc(x,y,w,h,start,sweep); CloseFigure(); }
        public void AddPie(Rectangle r,float s,float sw) => AddPie(r.X,r.Y,r.Width,r.Height,s,sw);
        public void AddPie(int x,int y,int w,int h,int s,int sw) => AddPie((float)x,(float)y,(float)w,(float)h,(float)s,(float)sw);

        public void AddPolygon(PointF[] pts) { AddLines(pts); CloseFigure(); }
        public void AddPolygon(Point[]  pts) { AddLines(pts); CloseFigure(); }

        public void AddBezier(float x1,float y1,float x2,float y2,float x3,float y3,float x4,float y4)
        {
            AddPoint(x1,y1, PathPointType.Start);
            AddPoint(x2,y2, PathPointType.Bezier3);
            AddPoint(x3,y3, PathPointType.Bezier3);
            AddPoint(x4,y4, PathPointType.Bezier3);
        }
        public void AddBezier(PointF p1,PointF p2,PointF p3,PointF p4) => AddBezier(p1.X,p1.Y,p2.X,p2.Y,p3.X,p3.Y,p4.X,p4.Y);
        public void AddBezier(Point  p1,Point  p2,Point  p3,Point  p4) => AddBezier(p1.X,p1.Y,p2.X,p2.Y,p3.X,p3.Y,p4.X,p4.Y);
        public void AddBeziers(PointF[] pts) { if(pts.Length<4) return; for(int i=0;i<=pts.Length-4;i+=3) AddBezier(pts[i],pts[i+1],pts[i+2],pts[i+3]); }
        public void AddBeziers(Point[]  pts) { if(pts.Length<4) return; for(int i=0;i<=pts.Length-4;i+=3) AddBezier(pts[i].X,pts[i].Y,pts[i+1].X,pts[i+1].Y,pts[i+2].X,pts[i+2].Y,pts[i+3].X,pts[i+3].Y); }

        public void AddCurve(PointF[] pts,float tension=0.5f) => AddSpline(pts,tension);
        public void AddCurve(PointF[] pts,int offset,int count,float tension) => AddSpline(pts,tension,offset,count);
        public void AddCurve(Point[]  pts,float tension=0.5f) { var pf = new PointF[pts.Length]; for(int i=0;i<pts.Length;i++) pf[i]=new PointF(pts[i].X,pts[i].Y); AddSpline(pf,tension); }
        public void AddClosedCurve(PointF[] pts,float tension=0.5f) { AddSpline(pts,tension); CloseFigure(); }
        public void AddClosedCurve(Point[]  pts,float tension=0.5f) { AddCurve(pts,tension); CloseFigure(); }

        public void AddPath(GraphicsPath path, bool connect)
        {
            _points.AddRange(path._points);
            _types.AddRange(path._types);
        }
        public void AddString(string s, FontFamily ff, int style, float emSize, PointF origin, StringFormat? format) { /* text path – not implemented */ }
        public void AddString(string s, FontFamily ff, int style, float emSize, RectangleF rect, StringFormat? format) { }

        // ------------------------------------------------------------------ //
        //  Flatten / transform                                                 //
        // ------------------------------------------------------------------ //

        public void Flatten() => Flatten(null, 0.25f);
        public void Flatten(Matrix? matrix, float flatness = 0.25f)
        {
            // Subdivide Bezier segments until they're straight enough.
            var newPts   = new List<PointF>();
            var newTypes = new List<byte>();
            int i = 0;
            while (i < _points.Count)
            {
                byte t = _types[i];
                byte kind = (byte)(t & (byte)PathPointType.PathTypeMask);
                if (kind == (byte)PathPointType.Bezier3 && i + 3 < _points.Count)
                {
                    FlattenBezier(_points[i],_points[i+1],_points[i+2],_points[i+3], newPts, newTypes, flatness);
                    i += 4;
                }
                else
                {
                    newPts.Add(_points[i]);
                    newTypes.Add(t);
                    i++;
                }
            }
            _points.Clear(); _points.AddRange(newPts);
            _types.Clear();  _types.AddRange(newTypes);
            if (matrix != null) { var arr = _points.ToArray(); matrix.TransformPoints(arr); _points.Clear(); _points.AddRange(arr); }
        }

        public void Transform(Matrix matrix) { var arr = _points.ToArray(); matrix.TransformPoints(arr); _points.Clear(); _points.AddRange(arr); }
        public void Warp(PointF[] destPts, RectangleF srcRect, Matrix? m=null, WarpMode mode=WarpMode.Perspective, float flatness=0.25f) { }
        public void Widen(Pen pen, Matrix? m=null, float flatness=0.25f) { }

        public RectangleF GetBounds(Matrix? m=null, Pen? p=null)
        {
            if (_points.Count == 0) return RectangleF.Empty;
            float minX=float.MaxValue, minY=float.MaxValue, maxX=float.MinValue, maxY=float.MinValue;
            foreach (var pt in _points) { if(pt.X<minX)minX=pt.X; if(pt.Y<minY)minY=pt.Y; if(pt.X>maxX)maxX=pt.X; if(pt.Y>maxY)maxY=pt.Y; }
            return new RectangleF(minX,minY,maxX-minX,maxY-minY);
        }

        public bool IsVisible(float x, float y) => GetBounds().Contains(x,y);
        public bool IsVisible(PointF pt) => IsVisible(pt.X,pt.Y);

        public void Reset() { _points.Clear(); _types.Clear(); }

        // ------------------------------------------------------------------ //
        //  Internal helpers                                                    //
        // ------------------------------------------------------------------ //

        private void AddPoint(float x, float y, PathPointType kind)
        {
            bool startNew = _points.Count == 0 || kind == PathPointType.Start;
            _points.Add(new PointF(x,y));
            _types.Add(startNew ? (byte)PathPointType.Start : (byte)kind);
        }
        private void AddPoint(float x, float y, byte kind) => AddPoint(x,y,(PathPointType)kind);

        private void AppendArcPoints(RectangleF rect, float startDeg, float sweepDeg)
        {
            int steps = Math.Max(4, (int)(Math.Abs(sweepDeg) / 5f));
            float cx = rect.X + rect.Width/2f, cy = rect.Y + rect.Height/2f;
            float rx = rect.Width/2f, ry = rect.Height/2f;
            float start = startDeg * MathF.PI / 180f, sweep = sweepDeg * MathF.PI / 180f;
            for (int i = 0; i <= steps; i++)
            {
                float a = start + sweep * i / steps;
                float x = cx + rx * MathF.Cos(a), y = cy + ry * MathF.Sin(a);
                AddPoint(x, y, i == 0 ? PathPointType.Start : PathPointType.Line);
            }
        }

        private void AddSpline(PointF[] pts, float tension, int offset = 0, int count = -1)
        {
            if (count < 0) count = pts.Length - offset;
            for (int i = offset; i < offset+count-1; i++)
            {
                PointF p0 = pts[Math.Max(0,i-1)], p1 = pts[i], p2 = pts[i+1], p3 = pts[Math.Min(pts.Length-1,i+2)];
                PointF c1 = new PointF(p1.X + tension*(p2.X-p0.X)/3f, p1.Y + tension*(p2.Y-p0.Y)/3f);
                PointF c2 = new PointF(p2.X - tension*(p3.X-p1.X)/3f, p2.Y - tension*(p3.Y-p1.Y)/3f);
                AddBezier(p1,c1,c2,p2);
            }
        }

        private static void FlattenBezier(PointF p0, PointF p1, PointF p2, PointF p3,
            List<PointF> pts, List<byte> types, float flatness)
        {
            // Check if the bezier is flat enough.
            float d1 = DistFromLine(p0,p3,p1), d2 = DistFromLine(p0,p3,p2);
            if (d1 < flatness && d2 < flatness)
            {
                pts.Add(p3); types.Add((byte)PathPointType.Line);
                return;
            }
            // Subdivide at t=0.5.
            PointF m01=Mid(p0,p1), m12=Mid(p1,p2), m23=Mid(p2,p3);
            PointF m012=Mid(m01,m12), m123=Mid(m12,m23), m0123=Mid(m012,m123);
            FlattenBezier(p0,m01,m012,m0123, pts,types,flatness);
            FlattenBezier(m0123,m123,m23,p3, pts,types,flatness);
        }
        private static PointF Mid(PointF a, PointF b) => new PointF((a.X+b.X)*0.5f,(a.Y+b.Y)*0.5f);
        private static float DistFromLine(PointF a, PointF b, PointF p)
        {
            float dx=b.X-a.X, dy=b.Y-a.Y, len=MathF.Sqrt(dx*dx+dy*dy);
            if (len<1e-6f) return MathF.Sqrt((p.X-a.X)*(p.X-a.X)+(p.Y-a.Y)*(p.Y-a.Y));
            return Math.Abs((dy*(p.X-a.X)-dx*(p.Y-a.Y))/len);
        }

        public object Clone()
        {
            var c = new GraphicsPath(FillMode);
            c._points.AddRange(_points); c._types.AddRange(_types); return c;
        }
        public void Dispose() { }
    }

    public sealed class GraphicsPathIterator : IDisposable
    {
        private readonly GraphicsPath _path;
        private int _subIdx;
        private int _pos;

        public GraphicsPathIterator(GraphicsPath path) { _path = path; }
        public int SubpathCount { get { int n=0; foreach(var t in _path.PathTypes) if((t&(byte)PathPointType.PathTypeMask)==(byte)PathPointType.Start) n++; return n; } }

        public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed)
        {
            var types = _path.PathTypes;
            startIndex = endIndex = _pos; isClosed = false;
            if (_pos >= types.Length) return 0;
            startIndex = _pos++;
            while (_pos < types.Length && (types[_pos]&(byte)PathPointType.PathTypeMask) != (byte)PathPointType.Start) _pos++;
            endIndex = _pos - 1;
            isClosed = (types[endIndex] & (byte)PathPointType.CloseSubpath) != 0;
            return endIndex - startIndex + 1;
        }

        public int NextSubpath(GraphicsPath path, out bool isClosed)
        {
            isClosed = false; return 0;
        }

        public void Dispose() { }
    }
}
