namespace PuzzleCore
{
    public class NotSameRule : IMoveRule
    {
        public bool Verify(Move move)
        {
            return move.From != move.To;
        }
    }
}
