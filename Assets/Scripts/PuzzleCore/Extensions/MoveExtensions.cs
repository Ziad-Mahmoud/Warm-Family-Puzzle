namespace PuzzleCore.Extensions
{
    public static class MoveExtensions
    {
        public static Move SetFrom(this Move move, GridElement from)
        {
            return new Move(from, move.To);
        }
        public static Move SetTo(this Move move, GridElement to)
        {
            return new Move(move.From, to);
        }
    }
}
