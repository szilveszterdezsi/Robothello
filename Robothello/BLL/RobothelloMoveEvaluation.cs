/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

using System;
using System.Collections.Generic;
using Robothello.DL;

namespace Robothello.BLL
{
    /// <summary>
    /// Wrapper class for the recursive implementation of the Minimax game tree search alogrithm with alpha-beta pruning.
    /// </summary>
    public class RobothelloMoveEvaluation
    {
        public int[][] NextGridState { get; private set; }
        public RobothelloCoordinate Move { get; }
        public Player Player { get; private set; }
        public DateTime EndTime { get; private set; }
        public int Depth { get; private set; }
        public int Nodes { get; private set; }
        public int Prune { get; private set; }
        public int Evaluation { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="currentGridState">Current data grid.</param>
        /// <param name="move">Current player move.</param>
        /// <param name="player">Current player.</param>
        /// <param name="endTime">Deadline (time allowed).</param>
        public RobothelloMoveEvaluation(int[][] currentGridState, RobothelloCoordinate move, Player player, DateTime endTime)
        {
            NextGridState = RobothelloGridTools.ApplyMoveOnDataGrid(currentGridState, move, player);
            Move = move;
            Player = player;
            EndTime = endTime;
            Depth = 0;
            Nodes = 0;
            Prune = 0;
            Evaluation = Minimax(NextGridState, 0, int.MinValue, int.MaxValue, Player, EndTime);
        }

        /// <summary>
        /// Recursive implementation of the Minimax game tree search alogrithm with alpha-beta pruning
        /// </summary>
        /// <param name="currentGridState">Current data grid.</param>
        /// <param name="depth">Start depth.</param>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="player">Current player.</param>
        /// <param name="endTime">Deadline (time allowed/remaining).</param>
        /// <returns></returns>
        private int Minimax(int[][] currentGridState, int depth, int alpha, int beta, Player player, DateTime endTime)
        {
            Depth = Math.Max(Depth, depth);
            if (DateTime.Now > endTime || RobothelloGridTools.GameOver(currentGridState))
            {
                return RobothelloGridTools.EvaluateGrid(currentGridState);
            }
            if (!RobothelloGridTools.PlayerCanMove(currentGridState, player))
            {
                player = (Player)((int)player * -1);
            }
            Nodes++;
            List<RobothelloCoordinate> possibleMoves = RobothelloGridTools.GetPossibleMoves(currentGridState, player);
            int possibleMovesCount = possibleMoves.Count;
            if (player == Player.AI)
            {
                int maxEval = int.MinValue;
                foreach (RobothelloCoordinate nextMove in possibleMoves)
                {
                    TimeSpan timeLeft = endTime - DateTime.Now;
                    DateTime newEndTime = endTime.AddMilliseconds(-timeLeft.Milliseconds / possibleMovesCount);
                    int[][] nextGridState = RobothelloGridTools.ApplyMoveOnDataGrid(currentGridState, nextMove, player);
                    int eval = Minimax(nextGridState, depth + 1, alpha, beta, (Player)((int)player * -1), newEndTime);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                    {
                        Prune++;
                        break;
                    }
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (RobothelloCoordinate nextMove in possibleMoves)
                {
                    TimeSpan timeLeft = endTime - DateTime.Now;
                    DateTime newEndTime = endTime.AddMilliseconds(-timeLeft.Milliseconds / possibleMovesCount);
                    int[][] nextGridState = RobothelloGridTools.ApplyMoveOnDataGrid(currentGridState, nextMove, player);
                    int eval = Minimax(nextGridState, depth + 1, alpha, beta, (Player)((int)player * -1), newEndTime);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                    {
                        Prune++;
                        break;
                    }
                }
                return minEval;
            }
        }
    }
}
