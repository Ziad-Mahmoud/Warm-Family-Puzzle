using System;

namespace PuzzleCore
{
    public class NearRule : IMoveRule
    {
        public bool Verify(Move move)
        {
            int fromX = move.From.currentIndex.x;
            int toX = move.To.currentIndex.x;

            int diffX = Math.Abs(fromX - toX);

            int fromY = move.From.currentIndex.y;
            int toY = move.To.currentIndex.y;

            int diffY = Math.Abs(fromY - toY);

            return diffX <= 1 && diffY <= 1;
        }
    }
}
