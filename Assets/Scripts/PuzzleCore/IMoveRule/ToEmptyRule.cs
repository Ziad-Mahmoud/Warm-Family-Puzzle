namespace PuzzleCore
{
    public class ToEmptyRule : IMoveRule
    {
        public bool Verify(Move move)
        {
            bool fromElement = !(move.From is EmptySpot);
            bool toEmptySpot = move.To is EmptySpot;
            return fromElement && toEmptySpot;
        }
    }
}
