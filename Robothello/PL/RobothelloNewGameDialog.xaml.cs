/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-27
/// Modified: n/a
/// ---------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using Robothello.DL;

namespace Robothello.PL
{
    /// <summary>
    /// Presentation dialog class used for generic I/O interaction with the user.
    /// </summary>
    public partial class RobothelloNewGameDialog : Window
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public RobothelloNewGameDialog()
        {
            InitializeComponent();
            PopulateComboBoxes();
        }

        /// <summary>
        /// Populates comboboxes components of dialog
        /// </summary>
        private void PopulateComboBoxes()
        {
            foreach (string gs in Enum.GetNames(typeof(GridSize)))
            {
                int tag = (int)(GridSize)Enum.Parse(typeof(GridSize), gs);
                cbGridSize.Items.Add(new ComboBoxItem() { Content = gs + " (" + tag + "x" + tag + ")", Tag = tag });
            }
            cbGridSize.SelectedIndex = 1;
            for (int i = 1; i <= 30; i++)
            {
                cbComputeTime.Items.Add(new ComboBoxItem() { Content = i + (i == 1 ? " second (default)" : " seconds"), Tag = i });
            }
            cbComputeTime.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets the selected grid size value.
        /// </summary>
        public int GridSize
        {
            get { return (int)((ComboBoxItem)cbGridSize.SelectedItem).Tag; }
        }

        /// <summary>
        /// Gets the selected AI compute time value.
        /// </summary>
        public int ComputeTime
        {
            get { return (int)((ComboBoxItem)cbComputeTime.SelectedItem).Tag; }
        }

        /// <summary>
        /// Value set flag for dialog.
        /// </summary>
        public bool ValueSet { get; set; }

        /// <summary>
        /// Detects when the "OK" button is clicked.
        /// Set ValueSet property "true" if "InputCheck" passes then closes itself.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (InputCheck())
            {
                ValueSet = true;
                Close();
            }
        }

        /// <summary>
        /// Detects when the "Canel" button is clicked.
        /// Set ValueSet property "false" then closes itself.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ValueSet = false;
            Close();
        }

        /// <summary>
        /// Checks if input is not empty.
        /// </summary>
        /// <returns>True if not empty, otherwise false.</returns>
        private bool InputCheck()
        {
            string title = "Incorrect Input";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage image = MessageBoxImage.Warning;
            if (cbGridSize.SelectedIndex == -1)
                MessageBox.Show("Please select a board size!", title, button, image);
            if (cbComputeTime.SelectedIndex == -1)
                MessageBox.Show("Please select AI compute time!", title, button, image);
            else
                return true;
            return false;
        }
    }
}
