using System.Collections.Generic;
using PuzzleCore;

namespace WarmFamilyPuzzle
{
    // Bridges the ported PuzzleCore engine to the Unity game.
    // Rule composition follows the original game-manager chain:
    // DistantMovePuzzleGame → DiagonalMovePuzzleGame →
    // HorizontalVerticalPuzzleGame → SlidingPuzzle (see GameMode).
    public class GameSession
    {
        public Puzzle Puzzle { get; private set; }
        public int Size { get; private set; }
        public GameMode Mode { get; private set; }
        public int Moves { get; private set; }
        public bool Assisted { get; private set; }

        readonly MoveHandler moveHandler = new MoveHandler();
        readonly PuzzleSolutionChecker solutionChecker = new PuzzleSolutionChecker();
        readonly List<Move> history = new List<Move>();

        public GameSession(int size, GameMode mode)
        {
            Size = size;
            Mode = mode;
            bool sliding = mode == GameMode.Sliding;

            var gridElements = new GridElement[size][];
            for (int x = 0; x < size; x++)
            {
                gridElements[x] = new GridElement[size];
                for (int y = 0; y < size; y++)
                {
                    var index = new PuzzleCore.Vector2(x, y);
                    int value = x * size + y + 1;
                    bool isLast = x == size - 1 && y == size - 1;
                    gridElements[x][y] = sliding && isLast
                        ? new EmptySpot(index, index, value)
                        : new GridElement(index, index, value);
                }
            }

            var moveRules = new List<IMoveRule> { new NotSameRule() };
            if (mode >= GameMode.NearSwap)
            {
                moveRules.Add(new NearRule());
            }

            if (mode >= GameMode.CrossSwap)
            {
                moveRules.Add(new NearEdgeRule());
            }

            if (sliding)
            {
                moveRules.Add(new ToEmptyRule());
            }

            Puzzle = new Puzzle(gridElements, moveRules);
        }

        public List<Move> Shuffle()
        {
            var shuffler = new PuzzleShuffler(Puzzle, moveHandler);
            var shuffleMoves = new List<Move>();
            for (int attempt = 0; attempt < 4 && IsSolved; attempt++)
            {
                shuffleMoves.AddRange(Mode == GameMode.Sliding
                    ? shuffler.Shuffle(Size * Size * 10)
                    : shuffler.ShuffleSwaps(Size * Size * 4));
            }

            history.AddRange(shuffleMoves);
            return shuffleMoves;
        }

        public GridElement EmptySpot
        {
            get
            {
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (Puzzle.GridElements[x][y] is EmptySpot)
                        {
                            return Puzzle.GridElements[x][y];
                        }
                    }
                }

                return null;
            }
        }

        public bool TrySwap(GridElement from, GridElement to)
        {
            var move = new Move(from, to);
            if (!moveHandler.VerifyPuzzleMove(move, Puzzle))
            {
                return false;
            }

            moveHandler.SwapElements(Puzzle, move);
            history.Add(move);
            Moves++;
            return true;
        }

        public bool TryMoveTile(GridElement tile)
        {
            GridElement empty = EmptySpot;
            return empty != null && TrySwap(tile, empty);
        }

        public bool IsSolved => solutionChecker.CheckWinCondition(Puzzle);

        public GridElement TileAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size)
            {
                return null;
            }

            return Puzzle.GridElements[x][y];
        }

        // Re-applying past swaps in reverse order walks the board back to the
        // solved state; used by Auto-solve. Marks the run as assisted so it
        // never counts as a best score.
        public List<Move> TakeAutoSolveMoves()
        {
            Assisted = true;
            var reversed = new List<Move>(history.Count);
            for (int i = history.Count - 1; i >= 0; i--)
            {
                reversed.Add(history[i]);
            }

            history.Clear();
            return reversed;
        }

        public void ApplyAutoSolveMove(Move move)
        {
            moveHandler.SwapElements(Puzzle, move);
        }
    }
}
