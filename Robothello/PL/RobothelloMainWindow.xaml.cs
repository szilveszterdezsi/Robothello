/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: n/a
/// ---------------------------

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Robothello.BLL;
using Robothello.DL;

namespace Robothello.PL
{
    /// <summary>
    /// Partial presentation class that initializes and handles GUI components and game controller.
    /// </summary>
    public partial class RobothelloMainWindow : Window
    {
        private RobothelloController GameController { get; set; }
        private RobothelloCellButton[][] CellGrid { get; set; }
        private RobothelloNewGameDialog NewGameDialog { get; set; }
        private Grid GUIGrid { get; set; }

        /// <summary>
        /// Constrctor that initializes GUI components and threads.
        /// </summary>
        public RobothelloMainWindow()
        {
            InitializeComponent();
            DataContext = this;
            GameController = new RobothelloController();
            GameController.AIMoveWorker.RunWorkerCompleted += AIRoundCompleted;
            GameController.AITimeWorker.ProgressChanged += SetAITime;
            if (GameController.DataGrid != null && !RobothelloGridTools.GameOver(GameController.DataGrid))
            {
                MessageBoxResult result = MessageBox.Show("Resume previous game?", "Resume Game", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeCellGrid(GameController.GridSize, GameController.DataGrid);
                    //MessageBox.Show("Game resumed!", "Game Resumed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    NewGame();
                }
            }
            else
            {
                NewGame();
            }
        }

        /// <summary>
        /// Opens a new game dialog and if player clicks "OK" a new game is started based on values set.
        /// </summary>
        private void NewGame()
        {
            NewGameDialog = new RobothelloNewGameDialog();
            NewGameDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NewGameDialog.Topmost = true;
            NewGameDialog.ShowDialog();
            if (NewGameDialog.ValueSet)
            {
                GameController.InitializeDataGrid(NewGameDialog.GridSize, NewGameDialog.ComputeTime);
                InitializeCellGrid(NewGameDialog.GridSize, GameController.DataGrid);
                ResetAITime();
                SetAIInfo("0");
                SetHumanInfo("AI: Let's play! You are black!");
                //MessageBox.Show("New game started!", "New Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Initializes the cell grid based on grid size (squared) and sets owners based on current data grid.
        /// If current player is AI an AI round is directly started, this only happens when resuming from a game
        /// that exited while AI move was still being calculated.
        /// </summary>
        /// <param name="gridSize">Current grid size (squared).</param>
        /// <param name="dataGrid">Current data grid.</param>
        private void InitializeCellGrid(int gridSize, int[][] dataGrid)
        {
            GUIGrid = new Grid();
            CellGrid = new RobothelloCellButton[gridSize][];
            for (int x = 0; x < gridSize; x++)
            {
                GUIGrid.ColumnDefinitions.Add(new ColumnDefinition());
                GUIGrid.RowDefinitions.Add(new RowDefinition());
                CellGrid[x] = new RobothelloCellButton[gridSize];
                for (int y = 0; y < gridSize; y++)
                {
                    CellGrid[x][y] = new RobothelloCellButton(new RobothelloCoordinate(x, y));
                    CellGrid[x][y].Click += GameCell_Click;
                    Grid.SetColumn(CellGrid[x][y], x);
                    Grid.SetRow(CellGrid[x][y], y);
                    GUIGrid.Children.Add(CellGrid[x][y]);
                }
            }
            Grid.SetColumn(GUIGrid, 0);
            Grid.SetRow(GUIGrid, 1);
            mainGrid.Children.Add(GUIGrid);
            SetCellGridOwners(dataGrid);
            if (GameController.CurrentPlayer == Player.AI)
            {
                AIRoundStarted(dataGrid);
                GameController.PlayAIRound();
            }
            else
            {
                SetCellGridEnabled(RobothelloGridTools.FindLegalMoves(dataGrid, Player.Human));
                UpdatePlayerDiscs(dataGrid);
            } 
        }

        /// <summary>
        /// Event that fires when a cell (button) coordinate is clicked by human player.
        /// Human round is played and applied and AI round follows.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void GameCell_Click(object sender, RoutedEventArgs e)
        {
            Task<int[][]> humanRound = new Task<int[][]>(() => GameController.PlayHumanRound((sender as RobothelloCellButton).GetCoord()));
            humanRound.Start();
            humanRound.Wait();
            AIRoundStarted(humanRound.Result);
            GameController.PlayAIRound();
        }

        /// <summary>
        /// Updates cell grid owners and scores as well as disables (locks) file-menu and cell grid while AI round is running.
        /// </summary>
        /// <param name="dataGrid">Current data grid.</param>
        private void AIRoundStarted(int[][] dataGrid)
        {
            miFile.IsEnabled = false;
            SetCellGridOwners(dataGrid);
            UpdatePlayerDiscs(dataGrid);
            SetCellGridEnabled(false);
            SetAIInfo("working");
        }

        /// <summary>
        /// Event that fires when AI round is completed.
        /// Updates cell grid owners and scores.
        /// Enables (unlocks) file-menu and cell grid with legal moves for current player.
        /// Checks is game is over.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void AIRoundCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            miFile.IsEnabled = true;
            int[][] dataGrid = GameController.DataGrid;
            SetCellGridOwners(dataGrid);
            UpdatePlayerDiscs(dataGrid);
            SetCellGridEnabled(RobothelloGridTools.FindLegalMoves(dataGrid, Player.Human));
            SetAIInfo(GameController.MaxDepth.ToString(), GameController.TotalNodes.ToString(), GameController.TotalPruned.ToString());
            if (RobothelloGridTools.GameOver(dataGrid))
            {
                GameOver();
            }
        }

        /// <summary>
        /// Set cell grid owners based on current data grid.
        /// </summary>
        /// <param name="dataGrid">Current data grid.</param>
        private void SetCellGridOwners(int[][] dataGrid)
        {
            for (int y = 0; y < CellGrid.Length; y++)
            {
                for (int x = 0; x < CellGrid[0].Length; x++)
                {
                    CellGrid[x][y].SetOwner((Player)dataGrid[x][y]);
                }
            }
        }

        /// <summary>
        /// Sets cell grid enabled based on boolean. 
        /// </summary>
        /// <param name="boolean">True enabled, false disabled.</param>
        private void SetCellGridEnabled(bool boolean)
        {
            foreach (RobothelloCellButton[] cellRow in CellGrid)
            {
                foreach (RobothelloCellButton cell in cellRow)
                {
                    cell.SetEnabled(boolean);
                }
            }
        }

        /// <summary>
        /// Sets cell grid enabled based on corresponding 2D boolean array. 
        /// </summary>
        /// <param name="boolean">True enabled, false disabled.</param>
        private void SetCellGridEnabled(bool[][] dataGrid)
        {
            for (int x = 0; x < CellGrid.Length; x++)
            {
                for (int y = 0; y < CellGrid[0].Length; y++)
                {
                    if (dataGrid[x][y])
                    {
                        CellGrid[x][y].SetEnabled(true);
                    }
                    else
                    {
                        CellGrid[x][y].SetEnabled(false);
                    }
                }
            }
        }

        /// <summary>
        /// Game is over and human player is informed about winner of the game and asked to play again.
        /// New game dialog is opened is player answers "Yes".
        /// </summary>
        private void GameOver()
        {
            string message;
            if (RobothelloGridTools.EvaluateGrid(GameController.DataGrid) < 0)
            {
                message = "AI: You beat me!";
            }
            else if (RobothelloGridTools.EvaluateGrid(GameController.DataGrid) > 0)
            {
                message = "AI: I beat you!";
            }
            else
            {
                message = "AI: We are evenly matched!";
            }
            SetHumanInfo(message);
            MessageBoxResult result = MessageBox.Show(message + "\nDo you want to play again?", "Game Over", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                NewGame();
            }
        }

        /// <summary>
        /// Updates player disc scores based on current data grid.
        /// </summary>
        /// <param name="dataGrid">Current data grid.</param>
        private void UpdatePlayerDiscs(int[][] dataGrid)
        {
            lblAIDiscs.Content = RobothelloGridTools.SearchGrid(dataGrid, Player.AI);
            lblHumanDiscs.Content = RobothelloGridTools.SearchGrid(dataGrid, Player.Human);
        }

        /// <summary>
        /// Sets human player info.
        /// </summary>
        /// <param name="info">Info message.</param>
        private void SetHumanInfo(string info)
        {
            lblHumanInfo.Content = info;
        }

        /// <summary>
        /// Sets AI info for depth, nodes and pruned labels.
        /// </summary>
        /// <param name="info">Info message.</param>
        private void SetAIInfo(string info)
        {
            SetAIInfo(info, info, info);
        }

        /// <summary>
        /// Sets AI info for depth, nodes and pruned labels.
        /// </summary>
        /// <param name="depthCount">Depth count.</param>
        /// <param name="nodeCount">Node count.</param>
        /// <param name="pruneCount">Pruned count.</param>
        private void SetAIInfo(string depthCount, string nodeCount, string pruneCount)
        {
            lblAIDepth.Content = depthCount;
            lblAINodes.Content = nodeCount;
            lblAIPrune.Content = pruneCount;
        }

        /// <summary>
        /// Updates AI compute time while AI round is running.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void SetAITime(object sender, ProgressChangedEventArgs e)
        {
            TimeSpan elapsed = (TimeSpan)e.UserState;
            Dispatcher.Invoke(() => {
                lblAITime.Content = $"{elapsed.Seconds}.{elapsed.Milliseconds:000}s";
            });
        }

        /// <summary>
        /// Resets AI compute time.
        /// </summary>
        private void ResetAITime()
        {
            lblAITime.Content = "0.000s";
        }
    }
}
