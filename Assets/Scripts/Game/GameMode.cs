namespace WarmFamilyPuzzle
{
    // The four rule sets from the original PuzzleCore game managers:
    // FreeSwap  = DistantMovePuzzleGame      (NotSame)
    // NearSwap  = DiagonalMovePuzzleGame     (NotSame + Near)
    // CrossSwap = HorizontalVerticalPuzzleGame (NotSame + Near + NearEdge)
    // Sliding   = SlidingPuzzle              (NotSame + Near + NearEdge + ToEmpty)
    public enum GameMode
    {
        FreeSwap = 0,
        NearSwap = 1,
        CrossSwap = 2,
        Sliding = 3
    }

    public static class GameModes
    {
        public static readonly string[] Short = { "Free", "Near", "Cross", "Slide" };
        public static readonly string[] Full = { "Free swap", "Near swap", "Cross swap", "Sliding" };

        public static string ShortName(this GameMode mode)
        {
            return Short[(int)mode];
        }

        public static string FullName(this GameMode mode)
        {
            return Full[(int)mode];
        }
    }
}
