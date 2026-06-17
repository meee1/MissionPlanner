// Font.cs  –  System.Drawing.Font / FontFamily backed by Unity Font when available.
// Falls back to a plain descriptor when Unity is not present.
using System.Drawing.Text;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace System.Drawing
{
    public sealed class FontFamily : IDisposable
    {
        public string Name { get; }

#if UNITY_ENGINE_PRESENT
        internal Font? UnityFont { get; }
        public FontFamily(string name) { Name = name; UnityFont = LoadUnityFont(name); }
        public FontFamily(GenericFontFamilies generic) : this(GenericName(generic)) { }
        private static Font? LoadUnityFont(string name)
        {
            var f = Resources.Load<Font>($"Fonts/{name}") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return f;
        }
#else
        public FontFamily(string name)               { Name = name; }
        public FontFamily(GenericFontFamilies generic){ Name = GenericName(generic); }
#endif

        private static string GenericName(GenericFontFamilies g) => g switch
        {
            GenericFontFamilies.Monospace => "Courier New",
            GenericFontFamilies.Serif     => "Times New Roman",
            _                             => "Arial"
        };

        public static FontFamily GenericMonospace => new FontFamily(GenericFontFamilies.Monospace);
        public static FontFamily GenericSerif      => new FontFamily(GenericFontFamilies.Serif);
        public static FontFamily GenericSansSerif  => new FontFamily(GenericFontFamilies.SansSerif);

        public bool IsStyleAvailable(FontStyle style) => true;
        public int  GetEmHeight(FontStyle style) => 2048;
        public int  GetCellAscent(FontStyle style) => 1854;
        public int  GetCellDescent(FontStyle style) => 434;
        public int  GetLineSpacing(FontStyle style) => 2355;

        public void Dispose() { }
    }

    public sealed class Font : IDisposable, ICloneable
    {
        public FontFamily FontFamily { get; }
        public string     Name       => FontFamily.Name;
        public float      Size       { get; }
        public FontStyle  Style      { get; }
        public GraphicsUnit Unit     { get; }
        public float      SizeInPoints => Unit == GraphicsUnit.Pixel ? Size * 0.75f : Size;
        public int        Height     => (int)Math.Ceiling(Size * 1.2f);
        public bool       Bold       => (Style & FontStyle.Bold)      != 0;
        public bool       Italic     => (Style & FontStyle.Italic)    != 0;
        public bool       Underline  => (Style & FontStyle.Underline) != 0;
        public bool       Strikeout  => (Style & FontStyle.Strikeout) != 0;

        // Unity font asset – null when not in a Unity environment.
#if UNITY_ENGINE_PRESENT
        internal UnityEngine.Font? UnityFont => FontFamily.UnityFont;
        internal UnityEngine.FontStyle UnityStyle =>
            Bold && Italic ? UnityEngine.FontStyle.BoldAndItalic :
            Bold           ? UnityEngine.FontStyle.Bold :
            Italic         ? UnityEngine.FontStyle.Italic :
                             UnityEngine.FontStyle.Normal;
        internal int UnitySizePixels => (int)Math.Round(
            Unit == GraphicsUnit.Pixel ? Size : Size * (96f / 72f));
#endif

        public Font(string name, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point, byte charSet = 0, bool vertical = false)
            : this(new FontFamily(name), size, style, unit) { }

        public Font(FontFamily family, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point, byte charSet = 0, bool vertical = false)
        { FontFamily = family; Size = size; Style = style; Unit = unit; }

        public Font(Font prototype, FontStyle newStyle)
            : this(prototype.FontFamily, prototype.Size, newStyle, prototype.Unit) { }

        public float GetHeight() => Height;
        public float GetHeight(float dpi) => Height * (dpi / 72f);
        public float GetHeight(Graphics g) => Height;

        public IntPtr ToHfont() => IntPtr.Zero;
        public static Font FromHfont(IntPtr h) => new Font("Arial", 12);

        public object Clone() => new Font(FontFamily, Size, Style, Unit);
        public void   Dispose() { }
    }
}
