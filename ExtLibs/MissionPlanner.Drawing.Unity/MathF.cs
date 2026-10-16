// MathF.cs – netstandard2.0 shim.
// MathF was added in netstandard2.1; provide the subset used in this project.
#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
namespace System
{
    internal static class MathF
    {
        public const float PI = (float)Math.PI;
        public const float E  = (float)Math.E;

        public static float Abs(float x)              => Math.Abs(x);
        public static float Ceiling(float x)          => (float)Math.Ceiling(x);
        public static float Cos(float x)              => (float)Math.Cos(x);
        public static float Floor(float x)            => (float)Math.Floor(x);
        public static float Log(float x)              => (float)Math.Log(x);
        public static float Log(float x, float b)     => (float)Math.Log(x, b);
        public static float Max(float a, float b)     => Math.Max(a, b);
        public static float Min(float a, float b)     => Math.Min(a, b);
        public static float Pow(float x, float y)     => (float)Math.Pow(x, y);
        public static float Round(float x)            => (float)Math.Round(x);
        public static float Sin(float x)              => (float)Math.Sin(x);
        public static float Sqrt(float x)             => (float)Math.Sqrt(x);
        public static float Tan(float x)              => (float)Math.Tan(x);
        public static float Truncate(float x)         => (float)Math.Truncate(x);
        public static float Atan2(float y, float x)   => (float)Math.Atan2(y, x);
        public static float Acos(float x)             => (float)Math.Acos(x);
        public static float Asin(float x)             => (float)Math.Asin(x);
        public static float Atan(float x)             => (float)Math.Atan(x);
        public static float Sign(float x)             => Math.Sign(x);
        public static float IEEERemainder(float x, float y) => (float)Math.IEEERemainder(x, y);
    }
}

// Math.Clamp overloads also missing from netstandard2.0
internal static class MathCompat
{
    public static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    public static int   Clamp(int   v, int   lo, int   hi) => v < lo ? lo : v > hi ? hi : v;
    public static double Clamp(double v, double lo, double hi) => v < lo ? lo : v > hi ? hi : v;
}
#endif
