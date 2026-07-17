using UnityEngine;

namespace WarmFamilyPuzzle
{
    // Design tokens taken from the "Sliding Puzzle" Claude design (PicSlide).
    public static class Theme
    {
        public static readonly Color Bg = Hex("#F3EDE2");
        public static readonly Color Card = Hex("#FFFDF8");
        public static readonly Color CardAlt = Hex("#FFFFFF");
        public static readonly Color Ink = Hex("#2B2320");
        public static readonly Color Muted = Hex("#8A7F76");
        public static readonly Color Coral = Hex("#E85D4A");
        public static readonly Color CoralDark = Hex("#C74534");
        public static readonly Color Teal = Hex("#2EA79B");
        public static readonly Color Yellow = Hex("#FFC93C");
        public static readonly Color Purple = Hex("#8B6FE8");
        public static readonly Color Gold = Hex("#B8860B");
        public static readonly Color Border = Hex("#F0E5D3");
        public static readonly Color ChipBg = Hex("#EFE6D8");
        public static readonly Color BoardBg = Hex("#EFE6D8");
        public static readonly Color EmptyCell = Hex("#E5DAC8");

        public static readonly Color[] Pastels =
        {
            Hex("#FFEDD8"), Hex("#FFD9E0"), Hex("#DFF3EE"),
            Hex("#FFF3D6"), Hex("#F1E9FF"), Hex("#FFE3DC")
        };

        public static readonly Color[] ConfettiColors =
        {
            Hex("#E85D4A"), Hex("#2EA79B"), Hex("#FFC93C"), Hex("#8B6FE8"), Hex("#FFD9E0")
        };

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
        }

        public static Color WithAlpha(this Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
