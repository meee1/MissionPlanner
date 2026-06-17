// StaticCollections.cs  –  Pens, Brushes, SystemPens, SystemBrushes, SystemColors,
//                          SystemFonts, SystemIcons.  Pure C#.

namespace System.Drawing
{
    // ------------------------------------------------------------------ //
    //  Pens                                                               //
    // ------------------------------------------------------------------ //
    public static class Pens
    {
        private static Pen P(Color c) => new Pen(c, 1f);
        public static Pen AliceBlue            => P(Color.AliceBlue);
        public static Pen AntiqueWhite         => P(Color.AntiqueWhite);
        public static Pen Aqua                 => P(Color.Aqua);
        public static Pen Aquamarine           => P(Color.Aquamarine);
        public static Pen Azure                => P(Color.Azure);
        public static Pen Beige                => P(Color.Beige);
        public static Pen Bisque               => P(Color.Bisque);
        public static Pen Black                => P(Color.Black);
        public static Pen BlanchedAlmond       => P(Color.BlanchedAlmond);
        public static Pen Blue                 => P(Color.Blue);
        public static Pen BlueViolet           => P(Color.BlueViolet);
        public static Pen Brown                => P(Color.Brown);
        public static Pen BurlyWood            => P(Color.BurlyWood);
        public static Pen CadetBlue            => P(Color.CadetBlue);
        public static Pen Chartreuse           => P(Color.Chartreuse);
        public static Pen Chocolate            => P(Color.Chocolate);
        public static Pen Coral                => P(Color.Coral);
        public static Pen CornflowerBlue       => P(Color.CornflowerBlue);
        public static Pen Cornsilk             => P(Color.Cornsilk);
        public static Pen Crimson              => P(Color.Crimson);
        public static Pen Cyan                 => P(Color.Cyan);
        public static Pen DarkBlue             => P(Color.DarkBlue);
        public static Pen DarkCyan             => P(Color.DarkCyan);
        public static Pen DarkGoldenrod        => P(Color.DarkGoldenrod);
        public static Pen DarkGray             => P(Color.DarkGray);
        public static Pen DarkGreen            => P(Color.DarkGreen);
        public static Pen DarkKhaki            => P(Color.DarkKhaki);
        public static Pen DarkMagenta          => P(Color.DarkMagenta);
        public static Pen DarkOliveGreen       => P(Color.DarkOliveGreen);
        public static Pen DarkOrange           => P(Color.DarkOrange);
        public static Pen DarkOrchid           => P(Color.DarkOrchid);
        public static Pen DarkRed              => P(Color.DarkRed);
        public static Pen DarkSalmon           => P(Color.DarkSalmon);
        public static Pen DarkSeaGreen         => P(Color.DarkSeaGreen);
        public static Pen DarkSlateBlue        => P(Color.DarkSlateBlue);
        public static Pen DarkSlateGray        => P(Color.DarkSlateGray);
        public static Pen DarkTurquoise        => P(Color.DarkTurquoise);
        public static Pen DarkViolet           => P(Color.DarkViolet);
        public static Pen DeepPink             => P(Color.DeepPink);
        public static Pen DeepSkyBlue          => P(Color.DeepSkyBlue);
        public static Pen DimGray              => P(Color.DimGray);
        public static Pen DodgerBlue           => P(Color.DodgerBlue);
        public static Pen Firebrick            => P(Color.Firebrick);
        public static Pen FloralWhite          => P(Color.FloralWhite);
        public static Pen ForestGreen          => P(Color.ForestGreen);
        public static Pen Fuchsia              => P(Color.Fuchsia);
        public static Pen Gainsboro            => P(Color.Gainsboro);
        public static Pen GhostWhite           => P(Color.GhostWhite);
        public static Pen Gold                 => P(Color.Gold);
        public static Pen Goldenrod            => P(Color.Goldenrod);
        public static Pen Gray                 => P(Color.Gray);
        public static Pen Green                => P(Color.Green);
        public static Pen GreenYellow          => P(Color.GreenYellow);
        public static Pen Honeydew             => P(Color.Honeydew);
        public static Pen HotPink              => P(Color.HotPink);
        public static Pen IndianRed            => P(Color.IndianRed);
        public static Pen Indigo               => P(Color.Indigo);
        public static Pen Ivory                => P(Color.Ivory);
        public static Pen Khaki                => P(Color.Khaki);
        public static Pen Lavender             => P(Color.Lavender);
        public static Pen LavenderBlush        => P(Color.LavenderBlush);
        public static Pen LawnGreen            => P(Color.LawnGreen);
        public static Pen LemonChiffon         => P(Color.LemonChiffon);
        public static Pen LightBlue            => P(Color.LightBlue);
        public static Pen LightCoral           => P(Color.LightCoral);
        public static Pen LightCyan            => P(Color.LightCyan);
        public static Pen LightGoldenrodYellow => P(Color.LightGoldenrodYellow);
        public static Pen LightGray            => P(Color.LightGray);
        public static Pen LightGreen           => P(Color.LightGreen);
        public static Pen LightPink            => P(Color.LightPink);
        public static Pen LightSalmon          => P(Color.LightSalmon);
        public static Pen LightSeaGreen        => P(Color.LightSeaGreen);
        public static Pen LightSkyBlue         => P(Color.LightSkyBlue);
        public static Pen LightSlateGray       => P(Color.LightSlateGray);
        public static Pen LightSteelBlue       => P(Color.LightSteelBlue);
        public static Pen LightYellow          => P(Color.LightYellow);
        public static Pen Lime                 => P(Color.Lime);
        public static Pen LimeGreen            => P(Color.LimeGreen);
        public static Pen Linen                => P(Color.Linen);
        public static Pen Magenta              => P(Color.Magenta);
        public static Pen Maroon               => P(Color.Maroon);
        public static Pen MediumAquamarine     => P(Color.MediumAquamarine);
        public static Pen MediumBlue           => P(Color.MediumBlue);
        public static Pen MediumOrchid         => P(Color.MediumOrchid);
        public static Pen MediumPurple         => P(Color.MediumPurple);
        public static Pen MediumSeaGreen       => P(Color.MediumSeaGreen);
        public static Pen MediumSlateBlue      => P(Color.MediumSlateBlue);
        public static Pen MediumSpringGreen    => P(Color.MediumSpringGreen);
        public static Pen MediumTurquoise      => P(Color.MediumTurquoise);
        public static Pen MediumVioletRed      => P(Color.MediumVioletRed);
        public static Pen MidnightBlue         => P(Color.MidnightBlue);
        public static Pen MintCream            => P(Color.MintCream);
        public static Pen MistyRose            => P(Color.MistyRose);
        public static Pen Moccasin             => P(Color.Moccasin);
        public static Pen NavajoWhite          => P(Color.NavajoWhite);
        public static Pen Navy                 => P(Color.Navy);
        public static Pen OldLace              => P(Color.OldLace);
        public static Pen Olive                => P(Color.Olive);
        public static Pen OliveDrab            => P(Color.OliveDrab);
        public static Pen Orange               => P(Color.Orange);
        public static Pen OrangeRed            => P(Color.OrangeRed);
        public static Pen Orchid               => P(Color.Orchid);
        public static Pen PaleGoldenrod        => P(Color.PaleGoldenrod);
        public static Pen PaleGreen            => P(Color.PaleGreen);
        public static Pen PaleTurquoise        => P(Color.PaleTurquoise);
        public static Pen PaleVioletRed        => P(Color.PaleVioletRed);
        public static Pen PapayaWhip           => P(Color.PapayaWhip);
        public static Pen PeachPuff            => P(Color.PeachPuff);
        public static Pen Peru                 => P(Color.Peru);
        public static Pen Pink                 => P(Color.Pink);
        public static Pen Plum                 => P(Color.Plum);
        public static Pen PowderBlue           => P(Color.PowderBlue);
        public static Pen Purple               => P(Color.Purple);
        public static Pen Red                  => P(Color.Red);
        public static Pen RosyBrown            => P(Color.RosyBrown);
        public static Pen RoyalBlue            => P(Color.RoyalBlue);
        public static Pen SaddleBrown          => P(Color.SaddleBrown);
        public static Pen Salmon               => P(Color.Salmon);
        public static Pen SandyBrown           => P(Color.SandyBrown);
        public static Pen SeaGreen             => P(Color.SeaGreen);
        public static Pen SeaShell             => P(Color.SeaShell);
        public static Pen Sienna               => P(Color.Sienna);
        public static Pen Silver               => P(Color.Silver);
        public static Pen SkyBlue              => P(Color.SkyBlue);
        public static Pen SlateBlue            => P(Color.SlateBlue);
        public static Pen SlateGray            => P(Color.SlateGray);
        public static Pen Snow                 => P(Color.Snow);
        public static Pen SpringGreen          => P(Color.SpringGreen);
        public static Pen SteelBlue            => P(Color.SteelBlue);
        public static Pen Tan                  => P(Color.Tan);
        public static Pen Teal                 => P(Color.Teal);
        public static Pen Thistle              => P(Color.Thistle);
        public static Pen Tomato               => P(Color.Tomato);
        public static Pen Transparent          => P(Color.Transparent);
        public static Pen Turquoise            => P(Color.Turquoise);
        public static Pen Violet               => P(Color.Violet);
        public static Pen Wheat                => P(Color.Wheat);
        public static Pen White                => P(Color.White);
        public static Pen WhiteSmoke          => P(Color.WhiteSmoke);
        public static Pen Yellow               => P(Color.Yellow);
        public static Pen YellowGreen          => P(Color.YellowGreen);
    }

    // ------------------------------------------------------------------ //
    //  Brushes                                                            //
    // ------------------------------------------------------------------ //
    public static class Brushes
    {
        private static SolidBrush B(Color c) => new SolidBrush(c);
        public static SolidBrush AliceBlue       => B(Color.AliceBlue);
        public static SolidBrush Aqua            => B(Color.Aqua);
        public static SolidBrush Azure           => B(Color.Azure);
        public static SolidBrush Beige           => B(Color.Beige);
        public static SolidBrush Black           => B(Color.Black);
        public static SolidBrush Blue            => B(Color.Blue);
        public static SolidBrush Brown           => B(Color.Brown);
        public static SolidBrush Chocolate       => B(Color.Chocolate);
        public static SolidBrush Coral           => B(Color.Coral);
        public static SolidBrush CornflowerBlue  => B(Color.CornflowerBlue);
        public static SolidBrush Crimson         => B(Color.Crimson);
        public static SolidBrush Cyan            => B(Color.Cyan);
        public static SolidBrush DarkBlue        => B(Color.DarkBlue);
        public static SolidBrush DarkGray        => B(Color.DarkGray);
        public static SolidBrush DarkGreen       => B(Color.DarkGreen);
        public static SolidBrush DarkOrange      => B(Color.DarkOrange);
        public static SolidBrush DarkRed         => B(Color.DarkRed);
        public static SolidBrush DimGray         => B(Color.DimGray);
        public static SolidBrush DodgerBlue      => B(Color.DodgerBlue);
        public static SolidBrush ForestGreen     => B(Color.ForestGreen);
        public static SolidBrush Fuchsia         => B(Color.Fuchsia);
        public static SolidBrush Gainsboro       => B(Color.Gainsboro);
        public static SolidBrush Gold            => B(Color.Gold);
        public static SolidBrush Gray            => B(Color.Gray);
        public static SolidBrush Green           => B(Color.Green);
        public static SolidBrush GreenYellow     => B(Color.GreenYellow);
        public static SolidBrush HotPink         => B(Color.HotPink);
        public static SolidBrush IndianRed       => B(Color.IndianRed);
        public static SolidBrush Ivory           => B(Color.Ivory);
        public static SolidBrush Khaki           => B(Color.Khaki);
        public static SolidBrush Lavender        => B(Color.Lavender);
        public static SolidBrush LightBlue       => B(Color.LightBlue);
        public static SolidBrush LightCyan       => B(Color.LightCyan);
        public static SolidBrush LightGray       => B(Color.LightGray);
        public static SolidBrush LightGreen      => B(Color.LightGreen);
        public static SolidBrush LightPink       => B(Color.LightPink);
        public static SolidBrush LightYellow     => B(Color.LightYellow);
        public static SolidBrush Lime            => B(Color.Lime);
        public static SolidBrush LimeGreen       => B(Color.LimeGreen);
        public static SolidBrush Magenta         => B(Color.Magenta);
        public static SolidBrush Maroon          => B(Color.Maroon);
        public static SolidBrush MediumBlue      => B(Color.MediumBlue);
        public static SolidBrush MidnightBlue    => B(Color.MidnightBlue);
        public static SolidBrush Navy            => B(Color.Navy);
        public static SolidBrush Olive           => B(Color.Olive);
        public static SolidBrush Orange          => B(Color.Orange);
        public static SolidBrush OrangeRed       => B(Color.OrangeRed);
        public static SolidBrush Orchid          => B(Color.Orchid);
        public static SolidBrush PaleGreen       => B(Color.PaleGreen);
        public static SolidBrush Pink            => B(Color.Pink);
        public static SolidBrush Plum            => B(Color.Plum);
        public static SolidBrush Purple          => B(Color.Purple);
        public static SolidBrush Red             => B(Color.Red);
        public static SolidBrush RoyalBlue       => B(Color.RoyalBlue);
        public static SolidBrush Salmon          => B(Color.Salmon);
        public static SolidBrush SeaGreen        => B(Color.SeaGreen);
        public static SolidBrush Silver          => B(Color.Silver);
        public static SolidBrush SkyBlue         => B(Color.SkyBlue);
        public static SolidBrush SlateGray       => B(Color.SlateGray);
        public static SolidBrush SpringGreen     => B(Color.SpringGreen);
        public static SolidBrush SteelBlue       => B(Color.SteelBlue);
        public static SolidBrush Tan             => B(Color.Tan);
        public static SolidBrush Teal            => B(Color.Teal);
        public static SolidBrush Thistle         => B(Color.Thistle);
        public static SolidBrush Tomato          => B(Color.Tomato);
        public static SolidBrush Transparent     => B(Color.Transparent);
        public static SolidBrush Turquoise       => B(Color.Turquoise);
        public static SolidBrush Violet          => B(Color.Violet);
        public static SolidBrush Wheat           => B(Color.Wheat);
        public static SolidBrush White           => B(Color.White);
        public static SolidBrush WhiteSmoke      => B(Color.WhiteSmoke);
        public static SolidBrush Yellow          => B(Color.Yellow);
        public static SolidBrush YellowGreen     => B(Color.YellowGreen);
    }

    // ------------------------------------------------------------------ //
    //  System collections (use sensible defaults)                         //
    // ------------------------------------------------------------------ //
    public static class SystemPens
    {
        public static Pen ActiveBorder        => new Pen(SystemColors.ActiveBorder);
        public static Pen ActiveCaption       => new Pen(SystemColors.ActiveCaption);
        public static Pen ButtonFace          => new Pen(SystemColors.ButtonFace);
        public static Pen ButtonShadow        => new Pen(SystemColors.ButtonShadow);
        public static Pen Control             => new Pen(SystemColors.Control);
        public static Pen ControlDark         => new Pen(SystemColors.ControlDark);
        public static Pen ControlDarkDark     => new Pen(SystemColors.ControlDarkDark);
        public static Pen ControlLight        => new Pen(SystemColors.ControlLight);
        public static Pen ControlLightLight   => new Pen(SystemColors.ControlLightLight);
        public static Pen ControlText         => new Pen(SystemColors.ControlText);
        public static Pen GrayText            => new Pen(SystemColors.GrayText);
        public static Pen Highlight           => new Pen(SystemColors.Highlight);
        public static Pen HighlightText       => new Pen(SystemColors.HighlightText);
        public static Pen HotTrack            => new Pen(SystemColors.HotTrack);
        public static Pen InactiveCaption     => new Pen(SystemColors.InactiveCaption);
        public static Pen Info                => new Pen(SystemColors.Info);
        public static Pen Menu                => new Pen(SystemColors.Menu);
        public static Pen MenuText            => new Pen(SystemColors.MenuText);
        public static Pen ScrollBar           => new Pen(SystemColors.ScrollBar);
        public static Pen Window              => new Pen(SystemColors.Window);
        public static Pen WindowFrame         => new Pen(SystemColors.WindowFrame);
        public static Pen WindowText          => new Pen(SystemColors.WindowText);
    }

    public static class SystemBrushes
    {
        public static SolidBrush ActiveBorder      => new SolidBrush(SystemColors.ActiveBorder);
        public static SolidBrush ButtonFace        => new SolidBrush(SystemColors.ButtonFace);
        public static SolidBrush Control           => new SolidBrush(SystemColors.Control);
        public static SolidBrush ControlDark       => new SolidBrush(SystemColors.ControlDark);
        public static SolidBrush ControlDarkDark   => new SolidBrush(SystemColors.ControlDarkDark);
        public static SolidBrush ControlLight      => new SolidBrush(SystemColors.ControlLight);
        public static SolidBrush ControlLightLight => new SolidBrush(SystemColors.ControlLightLight);
        public static SolidBrush ControlText       => new SolidBrush(SystemColors.ControlText);
        public static SolidBrush GrayText          => new SolidBrush(SystemColors.GrayText);
        public static SolidBrush Highlight         => new SolidBrush(SystemColors.Highlight);
        public static SolidBrush HighlightText     => new SolidBrush(SystemColors.HighlightText);
        public static SolidBrush Info              => new SolidBrush(SystemColors.Info);
        public static SolidBrush Menu              => new SolidBrush(SystemColors.Menu);
        public static SolidBrush MenuText          => new SolidBrush(SystemColors.MenuText);
        public static SolidBrush Window            => new SolidBrush(SystemColors.Window);
        public static SolidBrush WindowText        => new SolidBrush(SystemColors.WindowText);
    }

    public static class SystemColors
    {
        public static Color ActiveBorder        => Color.FromArgb(180,190,200);
        public static Color ActiveCaption       => Color.FromArgb( 51,153,255);
        public static Color ActiveCaptionText   => Color.White;
        public static Color AppWorkspace        => Color.FromArgb(171,171,171);
        public static Color ButtonFace          => Color.FromArgb(240,240,240);
        public static Color ButtonHighlight     => Color.White;
        public static Color ButtonShadow        => Color.FromArgb(160,160,160);
        public static Color Control             => Color.FromArgb(240,240,240);
        public static Color ControlDark         => Color.FromArgb(160,160,160);
        public static Color ControlDarkDark     => Color.FromArgb(105,105,105);
        public static Color ControlLight        => Color.FromArgb(227,227,227);
        public static Color ControlLightLight   => Color.White;
        public static Color ControlText         => Color.Black;
        public static Color Desktop             => Color.FromArgb( 0,  0,  0);
        public static Color GradientActiveCaption => Color.FromArgb(185,209,234);
        public static Color GradientInactiveCaption => Color.FromArgb(215,228,242);
        public static Color GrayText            => Color.FromArgb(109,109,109);
        public static Color Highlight           => Color.FromArgb( 0,120,215);
        public static Color HighlightText       => Color.White;
        public static Color HotTrack            => Color.FromArgb(  0,102,204);
        public static Color InactiveBorder      => Color.FromArgb(244,247,252);
        public static Color InactiveCaption     => Color.FromArgb(191,205,219);
        public static Color InactiveCaptionText => Color.FromArgb(  0,  0,  0);
        public static Color Info                => Color.FromArgb(255,255,225);
        public static Color InfoText            => Color.Black;
        public static Color Menu                => Color.FromArgb(240,240,240);
        public static Color MenuBar             => Color.FromArgb(240,240,240);
        public static Color MenuHighlight       => Color.FromArgb(  0,120,215);
        public static Color MenuText            => Color.Black;
        public static Color ScrollBar           => Color.FromArgb(200,200,200);
        public static Color Window              => Color.White;
        public static Color WindowFrame         => Color.FromArgb(100,100,100);
        public static Color WindowText          => Color.Black;
    }

    public static class SystemFonts
    {
        public static Font DefaultFont    => new Font("Arial", 9f);
        public static Font CaptionFont    => new Font("Arial", 9f, FontStyle.Bold);
        public static Font SmallCaptionFont => new Font("Arial", 8f);
        public static Font MenuFont       => new Font("Arial", 9f);
        public static Font MessageBoxFont => new Font("Arial", 9f);
        public static Font StatusFont     => new Font("Arial", 8f);
        public static Font IconTitleFont  => new Font("Arial", 9f);
        public static Font DialogFont     => new Font("Arial", 8.25f);
        public static Font GetFontByName(string name) => DefaultFont;
    }

    public static class SystemIcons
    {
        private static Icon Dummy => new Icon(new Bitmap(16,16));
        public static Icon Application => Dummy;
        public static Icon Asterisk    => Dummy;
        public static Icon Error       => Dummy;
        public static Icon Exclamation => Dummy;
        public static Icon Hand        => Dummy;
        public static Icon Information => Dummy;
        public static Icon Question    => Dummy;
        public static Icon Shield      => Dummy;
        public static Icon Warning     => Dummy;
        public static Icon WinLogo     => Dummy;
    }
}
