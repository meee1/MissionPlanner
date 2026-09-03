using System.ComponentModel;
using System.Globalization;

namespace System.Drawing
{
    /// <summary>
    /// Minimal port of <c>System.Drawing.FontConverter</c> for the SkiaSharp-backed shim.
    /// WinForms <c>.resx</c> files store a Font property as a string such as
    /// <c>"Microsoft Sans Serif, 8.25pt"</c> or <c>"Arial, 9.75pt, style=Bold, Italic"</c>, and
    /// <c>ComponentResourceManager.ApplyResources</c> reconstructs it via the Font type's
    /// <see cref="TypeConverter"/>. The shim's Font had no converter, so applying such a resource threw
    /// "TypeConverter cannot convert from System.String" — which aborted MainV2.InitializeComponent and
    /// stopped the main window from coming up. This converter parses that string format back into a Font.
    /// </summary>
    public class FontConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                s = s.Trim();
                if (s.Length == 0)
                    return null;

                if (culture == null) culture = CultureInfo.CurrentCulture;
                char sep = culture.TextInfo.ListSeparator[0]; // ',' in invariant/en cultures

                // Format: Name[<sep> size[unit]][<sep> style=Bold, Italic, ...]
                string[] parts = s.Split(sep);

                string familyName = parts.Length > 0 ? parts[0].Trim() : "Microsoft Sans Serif";
                float emSize = 8.25f;
                var style = FontStyle.Regular;

                if (parts.Length > 1)
                {
                    var sizeToken = parts[1].Trim();
                    // strip a trailing unit suffix (pt/px/...) — only point sizes are produced here.
                    int i = 0;
                    while (i < sizeToken.Length && (char.IsDigit(sizeToken[i]) || sizeToken[i] == '.' ||
                                                    sizeToken[i] == culture.NumberFormat.NumberDecimalSeparator[0]))
                        i++;
                    var numberPart = sizeToken.Substring(0, i);
                    float.TryParse(numberPart, NumberStyles.Float, culture, out emSize);
                    if (emSize <= 0)
                        emSize = 8.25f;
                }

                // Remaining parts may carry "style=Bold, Italic"
                for (int p = 2; p < parts.Length; p++)
                {
                    var token = parts[p].Trim();
                    var eq = token.IndexOf("style=", StringComparison.OrdinalIgnoreCase);
                    if (eq >= 0)
                        token = token.Substring(eq + "style=".Length).Trim();

                    if (token.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) >= 0) style |= FontStyle.Bold;
                    if (token.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) >= 0) style |= FontStyle.Italic;
                    if (token.IndexOf("Underline", StringComparison.OrdinalIgnoreCase) >= 0) style |= FontStyle.Underline;
                    if (token.IndexOf("Strikeout", StringComparison.OrdinalIgnoreCase) >= 0) style |= FontStyle.Strikeout;
                }

                return new Font(familyName, emSize, style, GraphicsUnit.Point);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string) && value is Font font)
            {
                if (culture == null) culture = CultureInfo.CurrentCulture;
                string sep = culture.TextInfo.ListSeparator + " ";
                var s = $"{font.Name}{sep}{font.SizeInPoints.ToString(culture)}pt";
                if (font.Style != FontStyle.Regular)
                    s += $"{sep}style={font.Style}";
                return s;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
