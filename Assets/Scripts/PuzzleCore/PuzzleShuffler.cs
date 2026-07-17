using System;
using System.Collections.Generic;

namespace PuzzleCore
{
    public class PuzzleShuffler
    {
        private readonly Puzzle puzzle;
        private readonly MoveHandler moveHandler;
        private readonly Random randomNumberGenerator = new Random();

        public PuzzleShuffler(Puzzle puzzle, MoveHandler moveHandler)
        {
            this.puzzle = puzzle;
            this.moveHandler = moveHandler;
        }

        // Shuffles by applying random rule-valid moves only, so the puzzle
        // always stays solvable (arbitrary swaps can create unsolvable states).
        public List<Move> Shuffle(int moveCount)
        {
            var history = new List<Move>(moveCount);
            GridElement lastMoved = null;

            for (int i = 0; i < moveCount; i++)
            {
                GridElement empty = FindEmptySpot();
                if (empty == null)
                {
                    break;
                }

                var candidates = new List<GridElement>(4);
                foreach (GridElement neighbor in NeighborsOf(empty))
                {
                    if (neighbor == lastMoved)
                    {
                        continue;
                    }

                    if (moveHandler.VerifyPuzzleMove(new Move(neighbor, empty), puzzle))
                    {
                        candidates.Add(neighbor);
                    }
                }

                if (candidates.Count == 0)
                {
                    continue;
                }

                GridElement chosen = candidates[randomNumberGenerator.Next(candidates.Count)];
                Move move = new Move(chosen, empty);
                moveHandler.SwapElements(puzzle, move);
                history.Add(move);
                lastMoved = chosen;
            }

            return history;
        }

        // Shuffle for the swap game modes (no empty spot): applies random
        // rule-valid pair swaps, so the board is always solvable in that mode.
        public List<Move> ShuffleSwaps(int swapCount)
        {
            var history = new List<Move>(swapCount);
            GridElement[][] grid = puzzle.GridElements;
            int rows = grid.Length;
            int guard = swapCount * 60;

            while (history.Count < swapCount && guard-- > 0)
            {
                GridElement a = grid[randomNumberGenerator.Next(rows)][randomNumberGenerator.Next(grid[0].Length)];
                GridElement b = grid[randomNumberGenerator.Next(rows)][randomNumberGenerator.Next(grid[0].Length)];
                Move move = new Move(a, b);
                if (!moveHandler.VerifyPuzzleMove(move, puzzle))
                {
                    continue;
                }

                moveHandler.SwapElements(puzzle, move);
                history.Add(move);
            }

            return history;
        }

        private GridElement FindEmptySpot()
        {
            GridElement[][] grid = puzzle.GridElements;
            for (int x = 0; x < grid.Length; x++)
            {
                for (int y = 0; y < grid[x].Length; y++)
                {
                    if (grid[x][y] is EmptySpot)
                    {
                        return grid[x][y];
                    }
                }
            }

            return null;
        }

        private IEnumerable<GridElement> NeighborsOf(GridElement element)
        {
            GridElement[][] grid = puzzle.GridElements;
            int x = element.currentIndex.x;
            int y = element.currentIndex.y;

            if (x > 0) yield return grid[x - 1][y];
            if (x < grid.Length - 1) yield return grid[x + 1][y];
            if (y > 0) yield return grid[x][y - 1];
            if (y < grid[x].Length - 1) yield return grid[x][y + 1];
        }
    }
}
