namespace PuzzleCore
{
    public class Move
    {
        public Move()
        {

        }
        public Move(GridElement from, GridElement to)
        {
            From = from;
            To = to;
        }
        public GridElement From { get; }
        public GridElement To { get; }
    }
}
