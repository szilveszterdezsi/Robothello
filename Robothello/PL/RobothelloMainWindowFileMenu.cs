/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-27
/// Modified: n/a
/// ---------------------------

using System;
using System.ComponentModel;
using System.Windows;

namespace Robothello.PL
{
    /// <summary>
    /// Partial presentation class that handles File-menu events and I/O interaction with the user.
    /// </summary>
    public partial class RobothelloMainWindow : Window
    {
        /// <summary>
        /// Check to confirm if user wants to end current game.
        /// </summary>
        /// <returns>True if user chooses "Yes", otherwise false.</returns>
        private bool EndCheck()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to end current game?", "End Game", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Detects when "New Game" is clicked in the File-menu and performs an 'EndCheck'.
        /// If ExitCheck returns true new game dialog will open.
        /// If attempt fails an error info message is displayed.
        /// </summary>
        /// <param name="sender">Component clicked.</param>
        /// <param name="e">Routed event.</param>
        private void NewCommand_Executed(object sender, RoutedEventArgs e)
        {
            if (EndCheck())
            {
                try
                {
                    NewGame();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Detects when "Exit" is clicked in the File-menu and exits.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void ExitCommand_Executed(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Overrides and detects when any shutdown event is triggered and performs an 'EndCheck'.
        /// If EndCheck returns true game exits.
        /// If EndCheck returns false exit is aborted.
        /// </summary>
        /// <param name="e">CancelEventArgs.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!EndCheck())
                e.Cancel = true;
        }
    }
}
