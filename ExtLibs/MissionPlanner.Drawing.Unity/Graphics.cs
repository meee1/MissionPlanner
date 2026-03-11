// Graphics.cs  –  System.Drawing.Graphics software rasterizer backed by a
// Bitmap (BGRA32 byte array).
//
// All drawing operations use pure C# pixel algorithms so this works on any
// Unity platform without an SkiaSharp dependency.
//
// Drawing pipeline:
//   Graphics.DrawLine / FillRectangle / etc.
//       → apply current affine Transform
//       → clip against ClipBounds
//       → write to _bmp._data (BGRA32 byte array)
//
// Pixel layout (BGRA32):  buf[idx*4+0]=B, buf[idx*4+1]=G, buf[idx*4+2]=R, buf[idx*4+3]=A

using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace System.Drawing
{
    public sealed class Graphics : IDisposable
    {
        // ------------------------------------------------------------------ //
        //  State                                                               //
        // ------------------------------------------------------------------ //

        private readonly Bitmap     _bmp;
        private          Matrix     _transform   = new Matrix();
        private          Rectangle  _clip;
        private          bool       _hasClip;
        private readonly Stack<(Matrix t, Rectangle c, bool hc)> _stateStack = new();

        // ------------------------------------------------------------------ //
        //  Properties                                                          //
        // ------------------------------------------------------------------ //

        public SmoothingMode     SmoothingMode     { get; set; } = SmoothingMode.Default;
        public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.Default;
        public TextRenderingHint TextRenderingHint { get; set; } = TextRenderingHint.SystemDefault;
        public CompositingMode   CompositingMode   { get; set; } = CompositingMode.SourceOver;
        public CompositingQuality CompositingQuality { get; set; } = CompositingQuality.Default;
        public PixelOffsetMode   PixelOffsetMode   { get; set; } = PixelOffsetMode.Default;
        public float             PageScale         { get; set; } = 1f;
        public GraphicsUnit      PageUnit          { get; set; } = GraphicsUnit.Pixel;
        public int               TextContrast      { get; set; } = 4;
        public float             DpiX              { get; } = 96f;
        public float             DpiY              { get; } = 96f;
        public Point             RenderingOrigin   { get; set; }

        public Matrix Transform
        {
            get => _transform;
            set => _transform = value ?? new Matrix();
        }

        public Region Clip
        {
            get => _hasClip ? new Region(_clip) : new Region();
            set { if (value == null || value.IsInfinite(this)) { _hasClip=false; } else { _clip = Rectangle.Round(value.Bounds); _hasClip=true; } }
        }
        public RectangleF ClipBounds       => _hasClip ? _clip : new RectangleF(0,0,_bmp.Width,_bmp.Height);
        public RectangleF VisibleClipBounds=> ClipBounds;
        public bool       IsClipEmpty      => _hasClip && (_clip.Width<=0||_clip.Height<=0);
        public bool       IsVisibleClipEmpty => IsClipEmpty;

        // ------------------------------------------------------------------ //
        //  Construction / factory                                              //
        // ------------------------------------------------------------------ //

        internal Graphics(Bitmap bmp) { _bmp = bmp; }

        public static Graphics FromImage(Image img)
        {
            var bmp = img as Bitmap ?? throw new ArgumentException("Image must be a Bitmap");
            return new Graphics(bmp);
        }

        // ------------------------------------------------------------------ //
        //  State save / restore                                                //
        // ------------------------------------------------------------------ //

        public GraphicsState Save()
        {
            _stateStack.Push(((Matrix)_transform.Clone(), _clip, _hasClip));
            return default;
        }
        public void Restore(GraphicsState state)
        {
            if (_stateStack.Count > 0)
            {
                var (t,c,hc) = _stateStack.Pop();
                _transform = t; _clip = c; _hasClip = hc;
            }
        }
        public GraphicsContainer BeginContainer() { Save(); return default; }
        public void EndContainer(GraphicsContainer c) => Restore(default);

        // ------------------------------------------------------------------ //
        //  Transform helpers                                                   //
        // ------------------------------------------------------------------ //

        public void ResetTransform()                  => _transform.Reset();
        public void TranslateTransform(float dx, float dy, MatrixOrder order=MatrixOrder.Prepend) => _transform.Translate(dx,dy,order);
        public void RotateTransform   (float a,           MatrixOrder order=MatrixOrder.Prepend) => _transform.Rotate(a,order);
        public void ScaleTransform    (float sx,float sy, MatrixOrder order=MatrixOrder.Prepend) => _transform.Scale(sx,sy,order);
        public void MultiplyTransform (Matrix m,          MatrixOrder order=MatrixOrder.Prepend) => _transform.Multiply(m,order);
        public void TranslateTransform(double dx, double dy) => _transform.Translate((float)dx,(float)dy);

        // ------------------------------------------------------------------ //
        //  Clip helpers                                                        //
        // ------------------------------------------------------------------ //

        public void SetClip(Rectangle rect,  CombineMode mode=CombineMode.Replace) { _clip=rect;  _hasClip=true; }
        public void SetClip(RectangleF rect, CombineMode mode=CombineMode.Replace) { _clip=Rectangle.Round(rect); _hasClip=true; }
        public void SetClip(Region region,   CombineMode mode=CombineMode.Replace) => Clip = region;
        public void SetClip(Graphics g,      CombineMode mode=CombineMode.Replace) { _hasClip=false; }
        public void SetClip(GraphicsPath path,CombineMode mode=CombineMode.Replace){ var b=path.GetBounds(); SetClip(b); }
        public void ResetClip()         { _hasClip=false; }
        public void ExcludeClip(Rectangle rect)  { /* simplified */ }
        public void ExcludeClip(Region region)   { /* simplified */ }
        public void IntersectClip(Rectangle r)   => SetClip(r, CombineMode.Intersect);
        public void IntersectClip(RectangleF r)  => SetClip(r, CombineMode.Intersect);
        public void IntersectClip(Region region) { var b=region.Bounds; IntersectClip(Rectangle.Round(b)); }
        public bool IsVisible(int x,int y)       => !_hasClip || _clip.Contains(x,y);
        public bool IsVisible(float x,float y)   => !_hasClip || _clip.Contains((int)x,(int)y);
        public bool IsVisible(Point pt)          => IsVisible(pt.X,pt.Y);
        public bool IsVisible(PointF pt)         => IsVisible(pt.X,pt.Y);
        public bool IsVisible(Rectangle rect)    => !_hasClip || _clip.IntersectsWith(rect);
        public bool IsVisible(RectangleF rect)   => !_hasClip || _clip.IntersectsWith(Rectangle.Round(rect));

        // ------------------------------------------------------------------ //
        //  Misc                                                                //
        // ------------------------------------------------------------------ //

        public void Clear(Color color)
        {
            byte b=color.B, g=color.G, r=color.R, a=color.A;
            var data = _bmp._data;
            for (int i=0; i<data.Length; i+=4) { data[i]=b; data[i+1]=g; data[i+2]=r; data[i+3]=a; }
        }
        public void Flush() { }
        public void Flush(FlushIntention intention) { }
        public IntPtr GetHdc() => IntPtr.Zero;
        public void   ReleaseHdc(IntPtr hdc) { }
        public void   ReleaseHdcInternal(IntPtr hdc) { }

        // MeasureString – approximates width as (fontSize * 0.6) * charCount.
        public SizeF MeasureString(string? s, Font font)
        {
            if (string.IsNullOrEmpty(s)) return SizeF.Empty;
            float w = font.Size * 0.6f * s.Length;
            float h = font.Size * 1.2f;
            return new SizeF(w,h);
        }
        public SizeF MeasureString(string? s, Font font, int width) => MeasureString(s,font);
        public SizeF MeasureString(string? s, Font font, SizeF size) => MeasureString(s,font);
        public SizeF MeasureString(string? s, Font font, int width, StringFormat? fmt) => MeasureString(s,font);
        public SizeF MeasureString(string? s, Font font, PointF origin, StringFormat? fmt) => MeasureString(s,font);
        public SizeF MeasureString(string? s, Font font, SizeF layoutArea, StringFormat? fmt,
            out int charFitted, out int linesFilled) { charFitted=s?.Length??0; linesFilled=1; return MeasureString(s,font); }

        public Region[] MeasureCharacterRanges(string? s, Font font, RectangleF rect, StringFormat? fmt)
            => Array.Empty<Region>();

        public void CopyFromScreen(int srcX,int srcY,int dstX,int dstY,Size sz) { }
        public void CopyFromScreen(Point src, Point dst, Size sz) { }
        public void CopyFromScreen(int srcX,int srcY,int dstX,int dstY,Size sz,CopyPixelOperation op) { }
        public enum CopyPixelOperation { SourceCopy=13369376 }

        // ------------------------------------------------------------------ //
        //  DrawImage                                                           //
        // ------------------------------------------------------------------ //

        public void DrawImage(Image img, int x,int y) => BlitBitmap(img,x,y,img.Width,img.Height,0,0,img.Width,img.Height);
        public void DrawImage(Image img, float x,float y) => DrawImage(img,(int)x,(int)y);
        public void DrawImage(Image img, Point pt) => DrawImage(img,pt.X,pt.Y);
        public void DrawImage(Image img, PointF pt) => DrawImage(img,(int)pt.X,(int)pt.Y);

        public void DrawImage(Image img, int x,int y,int w,int h) => BlitBitmap(img,x,y,w,h,0,0,img.Width,img.Height);
        public void DrawImage(Image img, float x,float y,float w,float h) => DrawImage(img,(int)x,(int)y,(int)w,(int)h);
        public void DrawImage(Image img, Rectangle rect) => DrawImage(img,rect.X,rect.Y,rect.Width,rect.Height);
        public void DrawImage(Image img, RectangleF rect) => DrawImage(img,(int)rect.X,(int)rect.Y,(int)rect.Width,(int)rect.Height);

        public void DrawImage(Image img, Rectangle dst, Rectangle src, GraphicsUnit unit)
            => BlitBitmap(img,dst.X,dst.Y,dst.Width,dst.Height,src.X,src.Y,src.Width,src.Height);
        public void DrawImage(Image img, RectangleF dst, RectangleF src, GraphicsUnit unit)
            => BlitBitmap(img,(int)dst.X,(int)dst.Y,(int)dst.Width,(int)dst.Height,(int)src.X,(int)src.Y,(int)src.Width,(int)src.Height);
        public void DrawImage(Image img, Rectangle dst, float sx,float sy,float sw,float sh, GraphicsUnit u, ImageAttributes? attr)
            => BlitBitmap(img,dst.X,dst.Y,dst.Width,dst.Height,(int)sx,(int)sy,(int)sw,(int)sh);
        public void DrawImage(Image img, Rectangle dst, int sx,int sy,int sw,int sh, GraphicsUnit u, ImageAttributes? attr)
            => BlitBitmap(img,dst.X,dst.Y,dst.Width,dst.Height,sx,sy,sw,sh);

        public void DrawImageUnscaled(Image img, int x,int y) => DrawImage(img,x,y);
        public void DrawImageUnscaled(Image img, Point pt)    => DrawImage(img,pt);
        public void DrawImageUnscaled(Image img, Rectangle r) => DrawImage(img,r);
        public void DrawImageUnscaled(Image img, int x,int y,int w,int h) => DrawImage(img,x,y,w,h);
        public void DrawImageUnscaledAndClipped(Image img, Rectangle rect) => DrawImage(img,rect);

        private void BlitBitmap(Image src, int dx,int dy,int dw,int dh, int sx,int sy,int sw,int sh)
        {
            var bmpSrc = src as Bitmap;
            if (bmpSrc == null) return;
            if (dw<=0||dh<=0||sw<=0||sh<=0) return;

            for (int py=0; py<dh; py++)
            for (int px=0; px<dw; px++)
            {
                int srcX = sx + (sw * px / dw);
                int srcY = sy + (sh * py / dh);
                var c = bmpSrc.GetPixel(srcX, srcY);
                SetPixelBlend(dx+px, dy+py, c);
            }
        }

        // ------------------------------------------------------------------ //
        //  DrawString / text                                                   //
        // ------------------------------------------------------------------ //

        public void DrawString(string? s, Font font, Brush brush, float x, float y) => DrawString(s,font,brush,x,y,null);
        public void DrawString(string? s, Font font, Brush brush, PointF pt) => DrawString(s,font,brush,pt.X,pt.Y,null);
        public void DrawString(string? s, Font font, Brush brush, float x, float y, StringFormat? fmt) => DrawStringImpl(s,font,brush,new RectangleF(x,y,float.MaxValue,float.MaxValue),fmt);
        public void DrawString(string? s, Font font, Brush brush, PointF pt, StringFormat? fmt) => DrawString(s,font,brush,pt.X,pt.Y,fmt);
        public void DrawString(string? s, Font font, Brush brush, RectangleF layout) => DrawStringImpl(s,font,brush,layout,null);
        public void DrawString(string? s, Font font, Brush brush, RectangleF layout, StringFormat? fmt) => DrawStringImpl(s,font,brush,layout,fmt);

        private void DrawStringImpl(string? text, Font font, Brush brush, RectangleF layout, StringFormat? fmt)
        {
            if (string.IsNullOrEmpty(text)) return;
            var color = brush is SolidBrush sb ? sb.Color : Color.Black;

#if UNITY_ENGINE_PRESENT
            DrawStringUnity(text, font, color, layout, fmt);
#else
            DrawStringFallback(text, font, color, layout, fmt);
#endif
        }

        // Fallback: draw tiny block-pixels per character using a built-in 3×5 font.
        private void DrawStringFallback(string text, Font font, Color color, RectangleF layout, StringFormat? fmt)
        {
            float charW = MathF.Max(3f, font.Size * 0.6f);
            float charH = MathF.Max(5f, font.Size);
            float x = layout.X, y = layout.Y;

            if (fmt != null)
            {
                if (fmt.Alignment == StringAlignment.Center) x += (layout.Width - charW*text.Length)/2f;
                else if (fmt.Alignment == StringAlignment.Far) x = layout.Right - charW*text.Length;
                if (fmt.LineAlignment == StringAlignment.Center) y += (layout.Height - charH)/2f;
                else if (fmt.LineAlignment == StringAlignment.Far) y = layout.Bottom - charH;
            }

            foreach (char c in text)
            {
                if (c == '\n') { x=layout.X; y+=charH+1; continue; }
                DrawMiniChar(c, (int)x, (int)y, (int)charW, (int)charH, color);
                x += charW;
                if (x + charW > layout.Right && layout.Width < float.MaxValue/2f) { x=layout.X; y+=charH+1; }
            }
        }

        // 3-pixel-wide × 5-pixel-tall stub glyphs (shows a tiny filled block per character).
        private void DrawMiniChar(char c, int x, int y, int w, int h, Color color)
        {
            // Draw a small filled rectangle to indicate character presence.
            for (int py=0; py<h-1; py++)
            for (int px=0; px<w-1; px++)
                SetPixelBlend(x+px, y+py, color);
        }

#if UNITY_ENGINE_PRESENT
        private void DrawStringUnity(string text, Font font, Color color, RectangleF layout, StringFormat? fmt)
        {
            var unityFont = font.UnityFont;
            if (unityFont == null) { DrawStringFallback(text,font,color,layout,fmt); return; }

            int pxSize = font.UnitySizePixels;
            unityFont.RequestCharactersInTexture(text, pxSize, font.UnityStyle);

            var fontTex = unityFont.material.mainTexture as Texture2D;
            bool readable = fontTex?.isReadable ?? false;
            if (!readable) { DrawStringFallback(text,font,color,layout,fmt); return; }

            Color32[] fontPixels  = fontTex!.GetPixels32();
            int       fontTexW    = fontTex.width;
            int       fontTexH    = fontTex.height;

            float curX = layout.X;
            float baselineY = layout.Y + pxSize;

            if (fmt != null)
            {
                float totalW = 0;
                foreach (char ch in text)
                    if (unityFont.GetCharacterInfo(ch, out var ci, pxSize, font.UnityStyle)) totalW += ci.advance;
                if (fmt.Alignment == StringAlignment.Center) curX = layout.X + (layout.Width - totalW)/2f;
                else if (fmt.Alignment == StringAlignment.Far) curX = layout.Right - totalW;
            }

            foreach (char ch in text)
            {
                if (!unityFont.GetCharacterInfo(ch, out var info, pxSize, font.UnityStyle)) { curX += pxSize*0.5f; continue; }

                // Glyph rect in font texture (UV → pixel coords).
                int gx = (int)(info.uvTopLeft.x  * fontTexW);
                int gy = (int)(info.uvTopLeft.y  * fontTexH);  // Unity: Y=0 is bottom
                int gw = (int)((info.uvTopRight.x - info.uvTopLeft.x) * fontTexW);
                int gh = (int)((info.uvTopLeft.y  - info.uvBottomLeft.y) * fontTexH);
                if (gw<=0||gh<=0) { curX+=info.advance; continue; }

                // Destination on our bitmap.
                int dstX = (int)(curX + info.minX);
                int dstY = (int)(baselineY - info.maxY);  // flip Y (Unity: +Y up, bitmap: +Y down)

                for (int py=0; py<gh; py++)
                for (int px=0; px<gw; px++)
                {
                    // Font texture Y is bottom-up.
                    int srcIdx = (gy - py) * fontTexW + (gx + px);
                    if ((uint)srcIdx >= (uint)fontPixels.Length) continue;
                    byte glyphAlpha = fontPixels[srcIdx].a;
                    if (glyphAlpha == 0) continue;
                    var c2 = Color.FromArgb((color.A * glyphAlpha + 127) / 255, color.R, color.G, color.B);
                    SetPixelBlend(dstX+px, dstY+py, c2);
                }
                curX += info.advance;
            }
        }
#endif

        // ------------------------------------------------------------------ //
        //  Lines                                                               //
        // ------------------------------------------------------------------ //

        public void DrawLine(Pen pen, float x1,float y1,float x2,float y2)
        {
            var p1 = TxPoint(x1,y1), p2 = TxPoint(x2,y2);
            DrawLineImpl((int)p1.X,(int)p1.Y,(int)p2.X,(int)p2.Y, pen.Color, pen.Width);
        }
        public void DrawLine(Pen pen, int x1,int y1,int x2,int y2) => DrawLine(pen,(float)x1,(float)y1,(float)x2,(float)y2);
        public void DrawLine(Pen pen, PointF p1, PointF p2) => DrawLine(pen,p1.X,p1.Y,p2.X,p2.Y);
        public void DrawLine(Pen pen, Point  p1, Point  p2) => DrawLine(pen,(float)p1.X,(float)p1.Y,(float)p2.X,(float)p2.Y);

        public void DrawLines(Pen pen, PointF[] pts) { for(int i=0;i<pts.Length-1;i++) DrawLine(pen,pts[i],pts[i+1]); }
        public void DrawLines(Pen pen, Point[]  pts) { for(int i=0;i<pts.Length-1;i++) DrawLine(pen,pts[i],pts[i+1]); }

        private void DrawLineImpl(int x0,int y0,int x1,int y1, Color color, float w)
        {
            if (w <= 1f)
            {
                BresenhamLine(x0,y0,x1,y1,color);
            }
            else
            {
                // Stroke the line with a perpendicular brush of the given width.
                float dx=x1-x0, dy=y1-y0, len=MathF.Sqrt(dx*dx+dy*dy);
                if (len < 0.5f) { FillCircle(x0,y0,(int)(w/2+0.5f),color); return; }
                float nx=-dy/len*w*0.5f, ny=dx/len*w*0.5f;
                var poly = new PointF[]
                {
                    new(x0+nx, y0+ny), new(x1+nx, y1+ny),
                    new(x1-nx, y1-ny), new(x0-nx, y0-ny)
                };
                FillPolygonImpl(poly, color);
            }
        }

        // ------------------------------------------------------------------ //
        //  Rectangles                                                          //
        // ------------------------------------------------------------------ //

        public void DrawRectangle(Pen pen, int x,int y,int w,int h) => DrawRectangle(pen,(float)x,(float)y,(float)w,(float)h);
        public void DrawRectangle(Pen pen, float x,float y,float w,float h)
        {
            DrawLine(pen,x,y,x+w,y); DrawLine(pen,x+w,y,x+w,y+h);
            DrawLine(pen,x+w,y+h,x,y+h); DrawLine(pen,x,y+h,x,y);
        }
        public void DrawRectangle(Pen pen, Rectangle r) => DrawRectangle(pen,r.X,r.Y,r.Width,r.Height);
        public void DrawRectangles(Pen pen, Rectangle[] rects)  { foreach(var r in rects) DrawRectangle(pen,r); }
        public void DrawRectangles(Pen pen, RectangleF[] rects) { foreach(var r in rects) DrawRectangle(pen,r.X,r.Y,r.Width,r.Height); }

        public void FillRectangle(Brush brush, int x,int y,int w,int h) => FillRectangle(brush,(float)x,(float)y,(float)w,(float)h);
        public void FillRectangle(Brush brush, float x,float y,float w,float h)
        {
            int x0=(int)x, y0=(int)y, x1=(int)(x+w), y1=(int)(y+h);
            for (int py=y0; py<y1; py++)
            for (int px=x0; px<x1; px++)
                SetPixelBlend(px,py, brush.GetColor(px,py,(int)w,(int)h));
        }
        public void FillRectangle(Brush brush, Rectangle r)  => FillRectangle(brush,r.X,r.Y,r.Width,r.Height);
        public void FillRectangle(Brush brush, RectangleF r) => FillRectangle(brush,r.X,r.Y,r.Width,r.Height);
        public void FillRectangles(Brush brush, Rectangle[]  rects) { foreach(var r in rects) FillRectangle(brush,r); }
        public void FillRectangles(Brush brush, RectangleF[] rects) { foreach(var r in rects) FillRectangle(brush,r); }

        // ------------------------------------------------------------------ //
        //  Ellipses                                                            //
        // ------------------------------------------------------------------ //

        public void DrawEllipse(Pen pen, float x,float y,float w,float h) => DrawEllipse(pen,new RectangleF(x,y,w,h));
        public void DrawEllipse(Pen pen, RectangleF rect)
        {
            float cx=rect.X+rect.Width/2f, cy=rect.Y+rect.Height/2f;
            float rx=rect.Width/2f, ry=rect.Height/2f;
            DrawEllipseOutline(cx,cy,rx,ry,pen.Color,pen.Width);
        }
        public void DrawEllipse(Pen pen, Rectangle rect) => DrawEllipse(pen,new RectangleF(rect.X,rect.Y,rect.Width,rect.Height));
        public void DrawEllipse(Pen pen, int x,int y,int w,int h) => DrawEllipse(pen,(float)x,(float)y,(float)w,(float)h);

        public void FillEllipse(Brush brush, float x,float y,float w,float h) => FillEllipse(brush,new RectangleF(x,y,w,h));
        public void FillEllipse(Brush brush, RectangleF rect)
        {
            float cx=rect.X+rect.Width/2f, cy=rect.Y+rect.Height/2f;
            float rx=rect.Width/2f, ry=rect.Height/2f;
            if (rx<=0||ry<=0) return;
            int y0=(int)(cy-ry), y1=(int)(cy+ry+0.5f);
            for (int py=y0; py<=y1; py++)
            {
                float dy=(py-cy)/ry;
                if (dy*dy>1f) continue;
                float span=rx*MathF.Sqrt(1f-dy*dy);
                int x0=(int)(cx-span), x1=(int)(cx+span+0.5f);
                for (int px=x0; px<=x1; px++)
                    SetPixelBlend(px,py, brush.GetColor(px,py,(int)(rx*2),(int)(ry*2)));
            }
        }
        public void FillEllipse(Brush brush, Rectangle rect)  => FillEllipse(brush,new RectangleF(rect.X,rect.Y,rect.Width,rect.Height));
        public void FillEllipse(Brush brush, int x,int y,int w,int h) => FillEllipse(brush,(float)x,(float)y,(float)w,(float)h);

        // ------------------------------------------------------------------ //
        //  Arcs and pies                                                       //
        // ------------------------------------------------------------------ //

        public void DrawArc(Pen pen, float x,float y,float w,float h,float startAngle,float sweepAngle)
            => DrawArcImpl(pen,new RectangleF(x,y,w,h),startAngle,sweepAngle);
        public void DrawArc(Pen pen, RectangleF rect,float s,float sw) => DrawArcImpl(pen,rect,s,sw);
        public void DrawArc(Pen pen, Rectangle  rect,float s,float sw) => DrawArcImpl(pen,new RectangleF(rect.X,rect.Y,rect.Width,rect.Height),s,sw);
        public void DrawArc(Pen pen, int x,int y,int w,int h,int s,int sw) => DrawArcImpl(pen,new RectangleF(x,y,w,h),(float)s,(float)sw);

        private void DrawArcImpl(Pen pen, RectangleF rect, float startDeg, float sweepDeg)
        {
            float cx=rect.X+rect.Width/2f,cy=rect.Y+rect.Height/2f,rx=rect.Width/2f,ry=rect.Height/2f;
            int steps=Math.Max(8,(int)(Math.Abs(sweepDeg)/2f));
            float startR=startDeg*MathF.PI/180f, sweepR=sweepDeg*MathF.PI/180f;
            PointF? prev=null;
            for (int i=0;i<=steps;i++)
            {
                float a=startR+sweepR*i/steps;
                var pt=TxPoint(cx+rx*MathF.Cos(a), cy+ry*MathF.Sin(a));
                if (prev.HasValue) DrawLineImpl((int)prev.Value.X,(int)prev.Value.Y,(int)pt.X,(int)pt.Y,pen.Color,pen.Width);
                prev=pt;
            }
        }

        public void DrawPie(Pen pen, float x,float y,float w,float h,float s,float sw) { DrawArc(pen,x,y,w,h,s,sw); var c=TxPoint(x+w/2,y+h/2); DrawLine(pen,c.X,c.Y,x+w/2+w/2*MathF.Cos(s*MathF.PI/180f),y+h/2+h/2*MathF.Sin(s*MathF.PI/180f)); DrawLine(pen,c.X,c.Y,x+w/2+w/2*MathF.Cos((s+sw)*MathF.PI/180f),y+h/2+h/2*MathF.Sin((s+sw)*MathF.PI/180f)); }
        public void DrawPie(Pen pen, RectangleF r,float s,float sw) => DrawPie(pen,r.X,r.Y,r.Width,r.Height,s,sw);
        public void DrawPie(Pen pen, Rectangle  r,float s,float sw) => DrawPie(pen,r.X,r.Y,r.Width,r.Height,s,sw);
        public void DrawPie(Pen pen, int x,int y,int w,int h,int s,int sw) => DrawPie(pen,(float)x,(float)y,(float)w,(float)h,(float)s,(float)sw);

        public void FillPie(Brush brush, float x,float y,float w,float h,float s,float sw)
        {
            float cx=x+w/2,cy=y+h/2,rx=w/2,ry=h/2;
            int steps=Math.Max(8,(int)(Math.Abs(sw)/2f));
            float startR=s*MathF.PI/180f,sweepR=sw*MathF.PI/180f;
            var pts=new PointF[steps+2];
            pts[0]=new PointF(cx,cy);
            for (int i=0;i<=steps;i++) { float a=startR+sweepR*i/steps; pts[i+1]=new PointF(cx+rx*MathF.Cos(a),cy+ry*MathF.Sin(a)); }
            FillPolygonPoints(brush,pts);
        }
        public void FillPie(Brush brush, Rectangle r,float s,float sw) => FillPie(brush,r.X,r.Y,r.Width,r.Height,s,sw);
        public void FillPie(Brush brush, int x,int y,int w,int h,int s,int sw) => FillPie(brush,(float)x,(float)y,(float)w,(float)h,(float)s,(float)sw);

        // ------------------------------------------------------------------ //
        //  Polygons                                                            //
        // ------------------------------------------------------------------ //

        public void DrawPolygon(Pen pen, PointF[] pts) { for(int i=0;i<pts.Length;i++) DrawLine(pen,pts[i],pts[(i+1)%pts.Length]); }
        public void DrawPolygon(Pen pen, Point[]  pts) { for(int i=0;i<pts.Length;i++) DrawLine(pen,pts[i],pts[(i+1)%pts.Length]); }

        public void FillPolygon(Brush brush, PointF[] pts, FillMode mode=FillMode.Alternate) => FillPolygonPoints(brush,pts);
        public void FillPolygon(Brush brush, Point[]  pts, FillMode mode=FillMode.Alternate)
        {
            var pf=new PointF[pts.Length]; for(int i=0;i<pts.Length;i++) pf[i]=new PointF(pts[i].X,pts[i].Y);
            FillPolygonPoints(brush,pf);
        }

        private void FillPolygonPoints(Brush brush, PointF[] pts)
        {
            var txd = new PointF[pts.Length];
            for (int i=0;i<pts.Length;i++) txd[i]=TxPoint(pts[i].X,pts[i].Y);
            FillPolygonImpl(txd, brush);
        }

        // ------------------------------------------------------------------ //
        //  Paths                                                               //
        // ------------------------------------------------------------------ //

        public void DrawPath(Pen pen, GraphicsPath path) { var pts=path.PathPoints; var types=path.PathTypes; DrawPathImpl(pts,types,pen,null); }
        public void FillPath(Brush brush, GraphicsPath path) { var pts=path.PathPoints; var types=path.PathTypes; DrawPathImpl(pts,types,null,brush); }

        private void DrawPathImpl(PointF[] pts, byte[] types, Pen? pen, Brush? brush)
        {
            if (pts.Length==0) return;
            var subPoly = new List<PointF>();
            PointF? bezierStart=null;

            void FlushSubPath()
            {
                if (subPoly.Count==0) return;
                if (brush!=null) FillPolygonPoints(brush, subPoly.ToArray());
                if (pen!=null)   DrawPolygon(pen, subPoly.Select(p=>new PointF(p.X,p.Y)).ToArray());
                subPoly.Clear();
            }

            for (int i=0;i<pts.Length;i++)
            {
                byte kind=(byte)(types[i]&(byte)PathPointType.PathTypeMask);
                switch (kind)
                {
                    case (byte)PathPointType.Start:
                        FlushSubPath(); subPoly.Add(pts[i]); break;
                    case (byte)PathPointType.Line:
                        subPoly.Add(pts[i]); break;
                    case (byte)PathPointType.Bezier3:
                        // Collect 3 bezier points
                        if (i+2 < pts.Length && (types[i+1]&7)==3 && (types[i+2]&7)==3)
                        {
                            var bpts = BezierFlatten(subPoly.Count>0?subPoly[subPoly.Count-1]:pts[i], pts[i],pts[i+1],pts[i+2]);
                            subPoly.AddRange(bpts); i+=2;
                        }
                        else subPoly.Add(pts[i]);
                        break;
                }
                if ((types[i]&(byte)PathPointType.CloseSubpath)!=0) FlushSubPath();
            }
            FlushSubPath();
        }

        // ------------------------------------------------------------------ //
        //  Curves and Beziers                                                  //
        // ------------------------------------------------------------------ //

        public void DrawBezier(Pen pen, float x1,float y1,float x2,float y2,float x3,float y3,float x4,float y4)
        {
            var flat=BezierFlatten(new PointF(x1,y1),new PointF(x2,y2),new PointF(x3,y3),new PointF(x4,y4));
            DrawLines(pen,flat);
        }
        public void DrawBezier(Pen pen, PointF p1,PointF p2,PointF p3,PointF p4) => DrawBezier(pen,p1.X,p1.Y,p2.X,p2.Y,p3.X,p3.Y,p4.X,p4.Y);
        public void DrawBezier(Pen pen, Point  p1,Point  p2,Point  p3,Point  p4) => DrawBezier(pen,p1.X,p1.Y,p2.X,p2.Y,p3.X,p3.Y,p4.X,p4.Y);

        public void DrawBeziers(Pen pen, PointF[] pts) { for(int i=0;i+3<pts.Length;i+=3) DrawBezier(pen,pts[i],pts[i+1],pts[i+2],pts[i+3]); }
        public void DrawBeziers(Pen pen, Point[]  pts) { for(int i=0;i+3<pts.Length;i+=3) DrawBezier(pen,pts[i].X,pts[i].Y,pts[i+1].X,pts[i+1].Y,pts[i+2].X,pts[i+2].Y,pts[i+3].X,pts[i+3].Y); }

        public void DrawCurve(Pen pen, PointF[] pts, float tension=0.5f) { DrawLines(pen, SplinePoints(pts,tension)); }
        public void DrawCurve(Pen pen, PointF[] pts, int offset, int count, float tension=0.5f) { var sub=pts.Skip(offset).Take(count).ToArray(); DrawCurve(pen,sub,tension); }
        public void DrawCurve(Pen pen, Point[]  pts, float tension=0.5f) { DrawLines(pen,pts); }
        public void DrawCurve(Pen pen, Point[]  pts, int offset, int count, float tension) { DrawCurve(pen,pts,tension); }

        public void DrawClosedCurve(Pen pen, PointF[] pts, float tension=0.5f, FillMode mode=FillMode.Alternate)
        { var sp=SplinePoints(pts,tension); DrawPolygon(pen,sp); }
        public void DrawClosedCurve(Pen pen, Point[]  pts, float tension=0.5f, FillMode mode=FillMode.Alternate)
        { DrawPolygon(pen,pts); }

        public void FillClosedCurve(Brush brush, PointF[] pts, FillMode mode=FillMode.Alternate, float tension=0.5f)
        { FillPolygon(brush,pts,mode); }
        public void FillClosedCurve(Brush brush, Point[] pts, FillMode mode=FillMode.Alternate, float tension=0.5f)
        { FillPolygon(brush,pts,mode); }

        // ------------------------------------------------------------------ //
        //  Icons                                                               //
        // ------------------------------------------------------------------ //

        public void DrawIcon(Icon icon, int x,int y) => DrawImage(icon.ToBitmap(),x,y);
        public void DrawIcon(Icon icon, Rectangle r) => DrawImage(icon.ToBitmap(),r);
        public void DrawIconUnstretched(Icon icon, Rectangle r) => DrawImage(icon.ToBitmap(),r.X,r.Y);

        // ------------------------------------------------------------------ //
        //  Misc draw                                                           //
        // ------------------------------------------------------------------ //

        public void DrawString(string? s, Font f, Brush b, int x,int y) => DrawString(s,f,b,(float)x,(float)y);

        // ------------------------------------------------------------------ //
        //  Internal pixel algorithms                                           //
        // ------------------------------------------------------------------ //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PointF TxPoint(float x, float y) => _transform.IsIdentity ? new PointF(x,y) : _transform.TransformPoint(x,y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPixelBlend(int x, int y, Color c)
        {
            if ((uint)x >= (uint)_bmp.Width || (uint)y >= (uint)_bmp.Height) return;
            if (_hasClip && !_clip.Contains(x,y)) return;

            var d = _bmp._data;
            int i = (y * _bmp.Stride) + (x * 4);
            if (c.A == 255 || CompositingMode == CompositingMode.SourceCopy)
            {
                d[i]=c.B; d[i+1]=c.G; d[i+2]=c.R; d[i+3]=c.A;
            }
            else if (c.A > 0)
            {
                int fa=c.A+1, fb=256-c.A;
                d[i]   = (byte)((c.B*fa + d[i]  *fb)>>8);
                d[i+1] = (byte)((c.G*fa + d[i+1]*fb)>>8);
                d[i+2] = (byte)((c.R*fa + d[i+2]*fb)>>8);
                d[i+3] = 255;
            }
        }

        private void BresenhamLine(int x0,int y0,int x1,int y1,Color color)
        {
            int dx=Math.Abs(x1-x0), dy=Math.Abs(y1-y0);
            int sx=x0<x1?1:-1, sy=y0<y1?1:-1;
            int err=dx-dy;
            while (true)
            {
                SetPixelBlend(x0,y0,color);
                if (x0==x1&&y0==y1) break;
                int e2=2*err;
                if (e2>-dy){ err-=dy; x0+=sx; }
                if (e2< dx){ err+=dx; y0+=sy; }
            }
        }

        private void FillCircle(int cx,int cy,int r,Color color)
        {
            for (int y=-r;y<=r;y++)
            for (int x=-r;x<=r;x++)
                if (x*x+y*y<=r*r) SetPixelBlend(cx+x,cy+y,color);
        }

        private void DrawEllipseOutline(float cx,float cy,float rx,float ry,Color color,float w)
        {
            int steps=Math.Max(32,(int)(2*MathF.PI*Math.Max(rx,ry)));
            PointF? prev=null;
            for (int i=0;i<=steps;i++)
            {
                float a=2f*MathF.PI*i/steps;
                var pt=TxPoint(cx+rx*MathF.Cos(a), cy+ry*MathF.Sin(a));
                if (prev.HasValue) DrawLineImpl((int)prev.Value.X,(int)prev.Value.Y,(int)pt.X,(int)pt.Y,color,w);
                prev=pt;
            }
        }

        // Scanline polygon fill (even-odd rule).
        private void FillPolygonImpl(PointF[] pts, Color color)
        {
            if (pts.Length<3) return;
            float minY=float.MaxValue, maxY=float.MinValue;
            foreach (var p in pts) { if(p.Y<minY)minY=p.Y; if(p.Y>maxY)maxY=p.Y; }
            int y0=(int)minY, y1=(int)(maxY+0.5f);
            var intersects=new List<float>(pts.Length);
            for (int scanY=y0; scanY<=y1; scanY++)
            {
                intersects.Clear();
                int n=pts.Length;
                for (int i=0,j=n-1;i<n;j=i++)
                {
                    float iy=pts[i].Y, jy=pts[j].Y;
                    if ((iy>scanY)!=(jy>scanY))
                        intersects.Add(pts[i].X+(scanY-iy)/(jy-iy)*(pts[j].X-pts[i].X));
                }
                intersects.Sort();
                for (int k=0;k+1<intersects.Count;k+=2)
                {
                    int xa=(int)intersects[k], xb=(int)(intersects[k+1]+0.5f);
                    for (int px=xa;px<=xb;px++) SetPixelBlend(px,scanY,color);
                }
            }
        }

        private void FillPolygonImpl(PointF[] pts, Brush brush)
        {
            if (pts.Length<3) return;
            float minY=float.MaxValue,maxY=float.MinValue,minX=float.MaxValue,maxX=float.MinValue;
            foreach(var p in pts){if(p.Y<minY)minY=p.Y;if(p.Y>maxY)maxY=p.Y;if(p.X<minX)minX=p.X;if(p.X>maxX)maxX=p.X;}
            int y0=(int)minY,y1=(int)(maxY+0.5f); int w=(int)(maxX-minX+1),h=(int)(maxY-minY+1);
            var intersects=new List<float>();
            for (int scanY=y0;scanY<=y1;scanY++)
            {
                intersects.Clear(); int n=pts.Length;
                for(int i=0,j=n-1;i<n;j=i++){float iy=pts[i].Y,jy=pts[j].Y; if((iy>scanY)!=(jy>scanY)) intersects.Add(pts[i].X+(scanY-iy)/(jy-iy)*(pts[j].X-pts[i].X));}
                intersects.Sort();
                for(int k=0;k+1<intersects.Count;k+=2){int xa=(int)intersects[k],xb=(int)(intersects[k+1]+0.5f); for(int px=xa;px<=xb;px++) SetPixelBlend(px,scanY,brush.GetColor(px,scanY,w,h));}
            }
        }

        // Flatten a cubic Bezier into line segments.
        private static PointF[] BezierFlatten(PointF p0,PointF p1,PointF p2,PointF p3, float flatness=1f)
        {
            var pts=new List<PointF>(); pts.Add(p0);
            BezierSubdivide(p0,p1,p2,p3,flatness,pts); return pts.ToArray();
        }
        private static void BezierSubdivide(PointF p0,PointF p1,PointF p2,PointF p3,float flatness,List<PointF> out_)
        {
            float d1=DistPtLine(p0,p3,p1), d2=DistPtLine(p0,p3,p2);
            if (d1<flatness&&d2<flatness) { out_.Add(p3); return; }
            var m01=Mid(p0,p1),m12=Mid(p1,p2),m23=Mid(p2,p3);
            var m012=Mid(m01,m12),m123=Mid(m12,m23),m=Mid(m012,m123);
            BezierSubdivide(p0,m01,m012,m,flatness,out_);
            BezierSubdivide(m,m123,m23,p3,flatness,out_);
        }
        private static PointF Mid(PointF a,PointF b)=>new PointF((a.X+b.X)*0.5f,(a.Y+b.Y)*0.5f);
        private static float DistPtLine(PointF a,PointF b,PointF p){float dx=b.X-a.X,dy=b.Y-a.Y,len=MathF.Sqrt(dx*dx+dy*dy);return len<1e-6f?0:Math.Abs((dy*(p.X-a.X)-dx*(p.Y-a.Y))/len);}

        // Catmull-Rom spline → polyline.
        private static PointF[] SplinePoints(PointF[] pts,float tension)
        {
            if (pts.Length<2) return pts;
            var out_=new List<PointF>(); out_.Add(pts[0]);
            for (int i=0;i<pts.Length-1;i++)
            {
                var p0=pts[Math.Max(0,i-1)],p1=pts[i],p2=pts[i+1],p3=pts[Math.Min(pts.Length-1,i+2)];
                var c1=new PointF(p1.X+tension*(p2.X-p0.X)/3f,p1.Y+tension*(p2.Y-p0.Y)/3f);
                var c2=new PointF(p2.X-tension*(p3.X-p1.X)/3f,p2.Y-tension*(p3.Y-p1.Y)/3f);
                out_.AddRange(BezierFlatten(p1,c1,c2,p2));
            }
            return out_.ToArray();
        }

        public void Dispose() { }
    }

    // Minimal stubs for types expected by callers.
    public struct GraphicsContainer { }
}
