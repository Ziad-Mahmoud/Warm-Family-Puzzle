using System.Collections.Generic;

namespace PuzzleCore.Extensions
{
    public static class PuzzleExtensions
    {
        public static Puzzle SetGridElements(this Puzzle puzzle, GridElement[][] gridElements)
        {
            return new Puzzle(gridElements, puzzle.moveRules);
        }
        public static Puzzle SetMoveRules(this Puzzle puzzle, List<IMoveRule> moveRules)
        {
            return new Puzzle(puzzle.GridElements, moveRules);
        }
    }
}
