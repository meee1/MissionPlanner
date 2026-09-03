using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing
{
    /// <summary>
    /// Minimal port of <c>System.Drawing.IconConverter</c> for the SkiaSharp-backed shim.
    /// An <see cref="Icon"/> resource in a WinForms <c>.resx</c> is stored as a <c>byte[]</c> blob whose
    /// resource header declares the type as <c>System.Drawing.Icon</c>. The resource reader reconstructs
    /// it via the type's <see cref="TypeConverter"/> and then verifies the result IS that declared type.
    /// Icon inherits Bitmap, so without its own converter it would fall back to <see cref="ImageConverter"/>
    /// which returns a <c>Bitmap</c> — the reader then throws BadImageFormatException
    /// ("Expected 'System.Drawing.Icon' but read 'System.Drawing.Bitmap'"). This converter returns an Icon.
    /// </summary>
    public class IconConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(byte[]) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(byte[]) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is byte[] bytes)
            {
                return new Icon(new MemoryStream(bytes));
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(byte[]))
            {
                if (value is Image image)
                {
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }

                return null;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
