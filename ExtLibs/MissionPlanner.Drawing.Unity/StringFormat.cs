// StringFormat.cs  –  Text layout options.  Pure C#.
namespace System.Drawing
{
    public sealed class StringFormat : IDisposable, ICloneable
    {
        public StringAlignment  Alignment     { get; set; } = StringAlignment.Near;
        public StringAlignment  LineAlignment { get; set; } = StringAlignment.Near;
        public StringTrimming   Trimming      { get; set; } = StringTrimming.Character;
        public StringFormatFlags FormatFlags  { get; set; } = 0;
        public HotkeyPrefix     HotkeyPrefix  { get; set; } = HotkeyPrefix.None;

        public StringFormat() { }
        public StringFormat(StringFormatFlags flags) { FormatFlags = flags; }
        public StringFormat(StringFormat proto)
        {
            Alignment     = proto.Alignment;
            LineAlignment = proto.LineAlignment;
            Trimming      = proto.Trimming;
            FormatFlags   = proto.FormatFlags;
        }

        public static StringFormat GenericDefault    => new StringFormat();
        public static StringFormat GenericTypographic => new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces);

        public void SetTabStops(float firstTabOffset, float[] tabStops) { }
        public void SetMeasurableCharacterRanges(CharacterRange[] ranges) { }

        public object Clone() => new StringFormat(this);
        public void   Dispose() { }
    }

    public struct CharacterRange
    {
        public int First  { get; set; }
        public int Length { get; set; }
        public CharacterRange(int first, int length) { First = first; Length = length; }
    }
}
