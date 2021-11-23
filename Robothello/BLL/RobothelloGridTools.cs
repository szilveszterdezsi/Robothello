/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

using System.Collections.Generic;
using Robothello.DL;

namespace Robothello.BLL
{
    /// <summary>
    /// Tool class containing static methods for manipulating and analying the Robothello data grid.
    /// </summary>
    public static class RobothelloGridTools
    {
        /// <summary>
        /// Static readonly property that contains coordinate offsets for all neighbouring cells.
        /// </summary>
        public static readonly RobothelloCoordinate[] Directions = new RobothelloCoordinate[] {
            new RobothelloCoordinate(1, 0),
            new RobothelloCoordinate(1, 1),
            new RobothelloCoordinate(0, 1),
            new RobothelloCoordinate(-1, 1),
            new RobothelloCoordinate(-1, 0),
            new RobothelloCoordinate(-1, -1),
            new RobothelloCoordinate(0, -1),
            new RobothelloCoordinate(1, -1),
        };

        /// <summary>
        /// Static method that returns an updated data grid with player move applied on current data grid.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <param name="move">Move coordinate to be applied.</param>
        /// <param name="player">Current player.</param>
        /// <returns>Updated data grid with move (cell coordinate) applied.</returns>
        public static int[][] ApplyMoveOnDataGrid(int[][] grid, RobothelloCoordinate move, Player player)
        {
            int[][] gridCopy = CopyGrid(grid);
            gridCopy[move.X][move.Y] = (int)player;
            List<RobothelloCoordinate> reverseList = new List<RobothelloCoordinate>();
            foreach (RobothelloCoordinate delta in Directions)
            {
                for (int x = move.X + delta.X, y = move.Y + delta.Y; OnGrid(x, gridCopy) && OnGrid(y, gridCopy); x += delta.X, y += delta.Y)
                {
                    if (gridCopy[x][y] == (int)Player.Empty)
                    {
                        break;
                    }
                    else if (gridCopy[x][y] == (int)player)
                    {
                        foreach (RobothelloCoordinate cell in reverseList)
                        {
                            gridCopy[cell.X][cell.Y] = (int)player;
                        }
                        break;
                    }
                    else
                    {
                        reverseList.Add(new RobothelloCoordinate(x, y));
                    }
                }
                reverseList.Clear();
            }
            return gridCopy;
        }

        /// <summary>
        /// Static method that returns a boolean on whether player can make any legal moves based on current data grid. 
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <param name="player">Current player.</param>
        /// <returns>True is player can move, otherwise false.</returns>
        public static bool PlayerCanMove(int[][] grid, Player player)
        {
            return (GetPossibleMoves(grid, player).Count > 0) ? true : false;
        }

        /// <summary>
        /// Static method that returns a list of possible moves for current player based on current data grid.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <param name="player">Current player.</param>
        /// <returns>List of possible moves.</returns>
        public static List<RobothelloCoordinate> GetPossibleMoves(int[][] grid, Player player)
        {
            bool[][] validMoves = FindLegalMoves(grid, player);
            List<RobothelloCoordinate> possibleMoves = new List<RobothelloCoordinate>();
            for (int x = 0; x < validMoves.Length; x++)
            {
                for (int y = 0; y < validMoves[x].Length; y++)
                {
                    if (validMoves[x][y])
                    {
                        possibleMoves.Add(new RobothelloCoordinate(x, y));
                    }
                }
            }
            return possibleMoves;
        }

        /// <summary>
        /// Static method that returns a 2D boolean array corresponding to current data grid of legal moves.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <param name="player">Current player.</param>
        /// <returns>2D array of legal moves. true = legal, false = not legal.</returns>
        public static bool[][] FindLegalMoves(int[][] grid, Player player)
        {
            bool endsWithPlayer = false, foundOpponent = false;
            bool[][] moveGrid = new bool[grid.Length][];
            for (int x = 0; x < moveGrid.Length; x++)
            {
                moveGrid[x] = new bool[grid.Length];
                for (int y = 0; y < moveGrid[x].Length; y++)
                {
                    if (grid[x][y] == (int)Player.Empty)
                    {
                        foreach (RobothelloCoordinate delta in Directions)
                        {
                            for (int i = x + delta.X, j = y + delta.Y; OnGrid(i, grid) && OnGrid(j, grid); i += delta.X, j += delta.Y)
                            {
                                if (grid[i][j] == (int)Player.Empty)
                                {
                                    break;
                                }
                                else if (grid[i][j] == (int)player)
                                {
                                    endsWithPlayer = true;
                                    break;
                                }
                                else
                                {
                                    foundOpponent = true;
                                }
                            }
                            if (foundOpponent && endsWithPlayer)
                            {
                                moveGrid[x][y] = true;
                            }
                            endsWithPlayer = false;
                            foundOpponent = false;
                        }
                    }
                }
            }
            return moveGrid;
        }

        /// <summary>
        /// Static method that returns a copy of the current data grid.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <returns>Copy of current data grid.</returns>
        public static int[][] CopyGrid(int[][] grid)
        {
            int[][] gridCopy = new int[grid.Length][];
            for (int x = 0; x < grid.Length; x++)
            {
                gridCopy[x] = new int[grid.Length];
                for (int y = 0; y < gridCopy[x].Length; y++)
                {
                    gridCopy[x][y] = grid[x][y];
                }
            }
            return gridCopy;
        }

        /// <summary>
        /// Static method that returns a boolean about whether a current row or column
        /// position is within the bounds of current data grid.
        /// </summary>
        /// <param name="position">Current row or column position.</param>
        /// <param name="grid">Current data grid.</param>
        /// <returns>True if yes, otherwise false.</returns>
        public static bool OnGrid(int position, int[][] grid)
        {
            return position >= 0 && position < grid.Length;
        }

        /// <summary>
        /// Static method that returns a boolean about whether any player can make any moves.
        /// If neither player can make any legal moves the game is over.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <returns>True is any player can move, otherwise false.</returns>
        public static bool GameOver(int[][] grid)
        {
            return (GetPossibleMoves(grid, Player.AI).Count > 0 || GetPossibleMoves(grid, Player.Human).Count > 0) ? false : true;
        }

        /// <summary>
        /// Static method that return a summary of the values of the data grid in order to determine
        /// the winner of the game. Negative = AI wins, positive = human wins and 0 = draw.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <returns>Summary current data grid values.</returns>
        public static int EvaluateGrid(int[][] grid)
        {
            int eval = 0;
            foreach (int[] cellRow in grid)
            {
                foreach (int cell in cellRow)
                {
                    eval += cell;
                }
            }
            return eval;
        }

        /// <summary>
        /// Static method that returns number of cells (discs) held by current player based on current data grid.
        /// </summary>
        /// <param name="grid">Current data grid.</param>
        /// <param name="player">Current player.</param>
        /// <returns>Current discs held by current player.</returns>
        public static int SearchGrid(int[][] grid, Player player)
        {
            int count = 0;
            foreach (int[] cellRow in grid)
            {
                foreach (int cell in cellRow)
                {
                    if (cell == (int)player)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
