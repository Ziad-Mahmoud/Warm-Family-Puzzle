namespace PuzzleCore
{
    public class MoveHandler
    {
        public void SwapElements(Puzzle puzzle, Move move)
        {
            Vector2 temp1 = move.From.currentIndex;
            Vector2 temp2 = move.To.currentIndex;

            (puzzle.GridElements[temp2.x][temp2.y], puzzle.GridElements[temp1.x][temp1.y]) =
                (puzzle.GridElements[temp1.x][temp1.y], puzzle.GridElements[temp2.x][temp2.y]);

            move.From.UpdateCurrentPosition(temp2);
            move.To.UpdateCurrentPosition(temp1);
        }

        public bool VerifyPuzzleMove(Move move, Puzzle puzzle)
        {
            foreach (var rule in puzzle.moveRules)
            {
                if (!rule.Verify(move))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
