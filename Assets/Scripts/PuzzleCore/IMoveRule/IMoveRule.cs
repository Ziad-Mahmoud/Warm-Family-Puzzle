namespace PuzzleCore
{
    public interface IMoveRule
    {
        bool Verify(Move move);
    }
}
