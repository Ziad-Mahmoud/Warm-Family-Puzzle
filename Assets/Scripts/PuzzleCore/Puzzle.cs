using System.Collections.Generic;

namespace PuzzleCore
{
    public class Puzzle
    {
        public readonly List<IMoveRule> moveRules;
        public GridElement[][] GridElements { get; private set; }
        public Puzzle()
        {

        }
        public Puzzle(GridElement[][] gridElements, List<IMoveRule> moveRules = null)
        {
            this.GridElements = gridElements;
            this.moveRules = moveRules;
        }
    }
}
