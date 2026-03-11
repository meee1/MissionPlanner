// Drawing2D/Matrix.cs  –  2-D affine transform matrix (3×3 homogeneous).
// Pure C#, no Unity/Skia dependency.

namespace System.Drawing.Drawing2D
{
    public sealed class Matrix : IDisposable, ICloneable
    {
        // Column-major 3×3:  [m11 m12 0]
        //                    [m21 m22 0]
        //                    [dx  dy  1]
        private float _m11, _m12, _m21, _m22, _dx, _dy;

        public float M11 { get => _m11; set => _m11 = value; }
        public float M12 { get => _m12; set => _m12 = value; }
        public float M21 { get => _m21; set => _m21 = value; }
        public float M22 { get => _m22; set => _m22 = value; }
        public float OffsetX => _dx;
        public float OffsetY => _dy;
        public float[] Elements => new[] { _m11, _m12, _m21, _m22, _dx, _dy };

        public bool IsIdentity =>
            Math.Abs(_m11-1f)<1e-5f && Math.Abs(_m12)<1e-5f &&
            Math.Abs(_m21)<1e-5f    && Math.Abs(_m22-1f)<1e-5f &&
            Math.Abs(_dx)<1e-5f     && Math.Abs(_dy)<1e-5f;

        public Matrix() { Reset(); }
        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
        { _m11=m11; _m12=m12; _m21=m21; _m22=m22; _dx=dx; _dy=dy; }
        public Matrix(double m11,double m12,double m21,double m22,double dx,double dy)
            : this((float)m11,(float)m12,(float)m21,(float)m22,(float)dx,(float)dy) { }
        public Matrix(RectangleF rect, PointF[] plgpts) { Reset(); /* simplified */ }

        public void Reset() { _m11=_m22=1f; _m12=_m21=_dx=_dy=0f; }

        // ------------------------------------------------------------------ //
        //  Transformations                                                     //
        // ------------------------------------------------------------------ //

        public void Translate(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
        {
            if (order == MatrixOrder.Prepend)
            { _dx += _m11*dx + _m21*dy; _dy += _m12*dx + _m22*dy; }
            else
            { _dx += dx; _dy += dy; }
        }
        public void Translate(double dx, double dy) => Translate((float)dx,(float)dy);

        public void Scale(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
        {
            if (order == MatrixOrder.Prepend)
            { _m11*=sx; _m12*=sx; _m21*=sy; _m22*=sy; }
            else
            { _m11*=sx; _m21*=sx; _m12*=sy; _m22*=sy; _dx*=sx; _dy*=sy; }
        }
        public void Scale(double sx, double sy) => Scale((float)sx,(float)sy);

        public void Rotate(float angle, MatrixOrder order = MatrixOrder.Prepend)
        {
            float r = angle * MathF.PI / 180f;
            float c = MathF.Cos(r), s = MathF.Sin(r);
            MultiplyBy(c, s, -s, c, 0, 0, order);
        }
        public void Rotate(double angle) => Rotate((float)angle);
        public void Rotate(double angle, MatrixOrder order) => Rotate((float)angle, order);

        public void RotateAt(float angle, PointF center, MatrixOrder order = MatrixOrder.Prepend)
        {
            Translate( center.X,  center.Y, order);
            Rotate(angle, order);
            Translate(-center.X, -center.Y, order);
        }
        public void RotateAt(double angle, PointF center) => RotateAt((float)angle,center);
        public void RotateAt(double angle,double cx,double cy) => RotateAt((float)angle,new PointF((float)cx,(float)cy));

        public void Shear(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
            => MultiplyBy(1, sy, sx, 1, 0, 0, order);

        public void Multiply(Matrix m, MatrixOrder order = MatrixOrder.Prepend)
            => MultiplyBy(m._m11, m._m12, m._m21, m._m22, m._dx, m._dy, order);

        public void Invert()
        {
            float det = _m11*_m22 - _m12*_m21;
            if (Math.Abs(det) < 1e-10f) return;
            float inv = 1f / det;
            float n11 =  _m22*inv, n12 = -_m12*inv;
            float n21 = -_m21*inv, n22 =  _m11*inv;
            float ndx = (_m21*_dy - _m22*_dx)*inv;
            float ndy = (_m12*_dx - _m11*_dy)*inv;
            _m11=n11; _m12=n12; _m21=n21; _m22=n22; _dx=ndx; _dy=ndy;
        }

        // ------------------------------------------------------------------ //
        //  Transform points                                                    //
        // ------------------------------------------------------------------ //

        public void TransformPoints(PointF[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new PointF(x*_m11 + y*_m21 + _dx, x*_m12 + y*_m22 + _dy);
            }
        }
        public void TransformPoints(Point[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new Point((int)(x*_m11+y*_m21+_dx),(int)(x*_m12+y*_m22+_dy));
            }
        }
        public void TransformVectors(PointF[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new PointF(x*_m11+y*_m21, x*_m12+y*_m22);
            }
        }

        public PointF TransformPoint(PointF p)
            => new PointF(p.X*_m11+p.Y*_m21+_dx, p.X*_m12+p.Y*_m22+_dy);
        public PointF TransformPoint(float x, float y)
            => new PointF(x*_m11+y*_m21+_dx, x*_m12+y*_m22+_dy);

        // ------------------------------------------------------------------ //

        private void MultiplyBy(float a11,float a12,float a21,float a22,float adx,float ady, MatrixOrder order)
        {
            float n11,n12,n21,n22,ndx,ndy;
            if (order == MatrixOrder.Prepend)
            {
                n11 = a11*_m11 + a12*_m21;  n12 = a11*_m12 + a12*_m22;
                n21 = a21*_m11 + a22*_m21;  n22 = a21*_m12 + a22*_m22;
                ndx = adx*_m11 + ady*_m21 + _dx;
                ndy = adx*_m12 + ady*_m22 + _dy;
            }
            else
            {
                n11 = _m11*a11 + _m12*a21;  n12 = _m11*a12 + _m12*a22;
                n21 = _m21*a11 + _m22*a21;  n22 = _m21*a12 + _m22*a22;
                ndx = _dx*a11  + _dy*a21 + adx;
                ndy = _dx*a12  + _dy*a22 + ady;
            }
            _m11=n11; _m12=n12; _m21=n21; _m22=n22; _dx=ndx; _dy=ndy;
        }

        public double Determinant => _m11*_m22 - _m12*_m21;

        public static Matrix operator *(Matrix a, Matrix b)
        {
            var r = (Matrix)a.Clone();
            r.Multiply(b, MatrixOrder.Append);
            return r;
        }

        public object Clone() => new Matrix(_m11,_m12,_m21,_m22,_dx,_dy);
        public void   Dispose() { }
    }
}
