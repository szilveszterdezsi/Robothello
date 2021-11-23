/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Robothello.DL;
using Robothello.DAL;

namespace Robothello.BLL
{
    /// <summary>
    /// Controller class of Robothello that serves the presentation layer.
    /// </summary>
    public class RobothelloController
    {
        private const string DefaultFilePath = "Robothello.save";
        public Player CurrentPlayer { get; private set; }
        public int[][] DataGrid { get; private set; }
        public int GridSize { get; private set; }
        private int AITime { get; set; }
        public BackgroundWorker AIMoveWorker { get; set; }
        public BackgroundWorker AITimeWorker { get; set; }
        private Stopwatch Stopwatch { get; set; }
        public int MaxDepth { get; private set; }
        public int TotalNodes { get; private set; }
        public int TotalPruned { get; private set; }
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        /// <summary>
        /// Empty constructor that initializes properties and event callbacks.
        /// If default save-file exists previous state is loaded to data grid.
        /// </summary>
        public RobothelloController()
        {
            if (File.Exists(DefaultFilePath))
            {
                Load();
            }
            AIMoveWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            AITimeWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            AIMoveWorker.DoWork += CalculateAIMove;
            AIMoveWorker.RunWorkerCompleted += CompletedAIMoveCalculation;
            AITimeWorker.DoWork += CalculateAITime;
            Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Initializes the 2D array data grid that handles which player owns which cell.
        /// Ownership of the first 4 center cells is also assigned as well as saves
        /// currect game state to default save-file.
        /// </summary>
        /// <param name="gridSize">Size of grid squared.</param>
        /// <param name="aiComputeTime">AI compute time in seconds.</param>
        public void InitializeDataGrid(int gridSize, int aiComputeTime)
        {
            GridSize = gridSize;
            AITime = aiComputeTime;
            DataGrid = new int[gridSize][];
            for (int x = 0; x < gridSize; x++)
            {
                DataGrid[x] = new int[gridSize];
            }
            DataGrid[gridSize / 2 - 1][gridSize / 2 - 1] = (int)Player.Human;
            DataGrid[gridSize / 2][gridSize / 2] = (int)Player.Human;
            DataGrid[gridSize / 2 - 1][gridSize / 2] = (int)Player.AI;
            DataGrid[gridSize / 2][gridSize / 2 - 1] = (int)Player.AI;
            Save();
        }

        /// <summary>
        /// Plays a round for the human player and applies it to the data grid as well as saves
        /// currect game state to default save-file.
        /// </summary>
        /// <param name="humanMove">Human move (cell coordinate) to be applied on data grid.</param>
        /// <returns>Data grid after move (cell coordinate) has been applied to data grid.</returns>
        public int[][] PlayHumanRound(RobothelloCoordinate humanMove)
        {
            DataGrid = RobothelloGridTools.ApplyMoveOnDataGrid(DataGrid, humanMove, Player.Human);
            CurrentPlayer = Player.AI;
            Save();
            return DataGrid;
        }

        /// <summary>
        /// Plays a round for the AI player.
        /// BackgroundWorker threads are started for calculating and applying AI move (cell coordinate)
        /// to data grid as well as callback for time elasped during calculating move.
        /// </summary>
        public void PlayAIRound()
        {
            AIMoveWorker.RunWorkerAsync();
            AITimeWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Event that fires when AI round thread is started.
        /// Best AI move (cell coordinate) is calculated and applied to data grid, if AI has legal moves to make.
        /// While human doesn't have following legal moves the AI will keep playing rounds, if it has legal moves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateAIMove(object sender, DoWorkEventArgs e)
        {
            if (RobothelloGridTools.PlayerCanMove(DataGrid, Player.AI))
            {
                StartTime = DateTime.Now;
                EndTime = StartTime.AddSeconds(AITime);
                RobothelloCoordinate aiMove = GetBestAIMove(DataGrid, EndTime);
                DataGrid = RobothelloGridTools.ApplyMoveOnDataGrid(DataGrid, aiMove, Player.AI);
            }
            while (!RobothelloGridTools.PlayerCanMove(DataGrid, Player.Human) && RobothelloGridTools.PlayerCanMove(DataGrid, Player.AI))
            {
                Stopwatch.Reset();
                StartTime = DateTime.Now;
                EndTime = StartTime.AddSeconds(AITime);
                RobothelloCoordinate aiMove = GetBestAIMove(DataGrid, EndTime);
                DataGrid = RobothelloGridTools.ApplyMoveOnDataGrid(DataGrid, aiMove, Player.AI);
            }
        }

        /// <summary>
        /// Calculates all possibles AI moves and evalutes best move in the time allowed.
        /// Possible moves are randomized by sorting and ordering moves by GUID.
        /// Moves are evaluated in paralell to make full use of CPU with multiple cores.
        /// RobothelloMoveEvaluation is a wrapper class for the MiniMax-algorithm used to evalute the best
        /// move by looking forward in all "directions/dimentions" of the game.
        /// </summary>
        /// <param name="currentGridState">Current state of data grid.</param>
        /// <param name="endTime">DateTime object used for deadline of time allowed for calculating move.</param>
        /// <returns>Best AI move (cell coordinate).</returns>
        private RobothelloCoordinate GetBestAIMove(int[][] currentGridState, DateTime endTime)
        {
            List<RobothelloCoordinate> possibleMoves = RobothelloGridTools.GetPossibleMoves(currentGridState, Player.AI);
            possibleMoves = possibleMoves.OrderBy(r => Guid.NewGuid()).ToList();
            //Console.WriteLine("AI has " + possibleMoves.Count + " possible moves:");
            List<RobothelloMoveEvaluation> moveEvaluations = new List<RobothelloMoveEvaluation>();
            ThreadPool.SetMinThreads(possibleMoves.Count+1, possibleMoves.Count+1);
            Parallel.ForEach(possibleMoves, move =>
            {
                moveEvaluations.Add(new RobothelloMoveEvaluation(currentGridState, move, Player.AI, endTime));
            });
            //foreach (RobothelloMoveEvaluation eval in moveEvaluations)
            //{
            //    Console.WriteLine("[x=" + eval.Move.X + ", y=" + eval.Move.Y + "] eval:" + eval.Evaluation + " / depth:" + eval.Depth + " / nodes:" + eval.Nodes + " / pruned:" + eval.Prune);
            //}
            MaxDepth = moveEvaluations.OrderByDescending(i => i.Depth).FirstOrDefault().Depth;
            TotalNodes = moveEvaluations.Sum(evaluation => evaluation.Nodes);
            TotalPruned = moveEvaluations.Sum(evaluation => evaluation.Prune);
            //Console.WriteLine("max depth: " + MaxDepth + "\ntotal nodes examined:" + TotalNodes + "\ntotal branches pruned: " + TotalPruned);
            RobothelloCoordinate bestMove = moveEvaluations.OrderByDescending(move => move.Evaluation).FirstOrDefault().Move;
            //Console.WriteLine("AI chose move: [x=" + bestMove.X + ", y=" + bestMove.Y + "]\n");
            return bestMove;
        }

        /// <summary>
        /// Event that fires when AI round thread is completed that cancels callback of time
        /// elasped during calculating move as well as saves currect game state to default save-file.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void CompletedAIMoveCalculation(object sender, RunWorkerCompletedEventArgs e)
        {
            AIMoveWorker.CancelAsync();
            AITimeWorker.CancelAsync();
            CurrentPlayer = Player.Human;
            Save();
        }

        /// <summary>
        /// Event that fires while AI round calculation time it updated.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void CalculateAITime(object sender, DoWorkEventArgs e)
        {
            Stopwatch.Start();
            while (!AITimeWorker.CancellationPending)
            {
                //Thread.CurrentThread.Priority = ThreadPriority.Highest;
                AITimeWorker.ReportProgress(0, Stopwatch.Elapsed);
                Thread.Sleep(10);
            }
            AITimeWorker.ReportProgress(0, Stopwatch.Elapsed);
            Stopwatch.Reset();
        }

        /// <summary>
        /// Loads the currect game state from the default save-file.
        /// </summary>
        public void Load()
        {
            try
            {
                List<dynamic> items = RobothelloSerialization.BinaryDeserializeFromFile<List<dynamic>>(DefaultFilePath);
                CurrentPlayer = items[0];
                GridSize = items[1];
                DataGrid = items[2];
                AITime = items[3];
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves the currect game state to the default save-file.
        /// </summary>
        public void Save()
        {
            try
            {
                List<dynamic> items = new List<dynamic>() { CurrentPlayer, GridSize, DataGrid, AITime }; //<--- add objects to be saved to this list
                RobothelloSerialization.BinarySerializeToFile(items, DefaultFilePath);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
