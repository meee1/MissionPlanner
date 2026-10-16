using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing
{
    /// <summary>
    /// Minimal port of <c>System.Drawing.ImageConverter</c> for the SkiaSharp-backed shim.
    /// WinForms <c>.resx</c> files store images as a <c>byte[]</c> blob and the resource reader
    /// (<c>System.Resources.Extensions.DeserializingResourceReader</c>) reconstructs them by calling
    /// the Image type's <see cref="TypeConverter"/> with that <c>byte[]</c>. Without a converter that
    /// accepts <c>byte[]</c> the reader throws "TypeConverter cannot convert from System.Byte[]"
    /// (which crashed Splash.InitializeComponent on the MAUI Windows head).
    /// </summary>
    public class ImageConverter : TypeConverter
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
                return new Bitmap(new MemoryStream(bytes));
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
