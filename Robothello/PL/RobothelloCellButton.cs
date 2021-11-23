/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Robothello.DL;

namespace Robothello.PL
{
    /// <summary>
    /// Button class customized for the Robothello game and used to display and handle a cell (disc) and it's owner.
    /// </summary>
    public class RobothelloCellButton : Button
    {
        private RobothelloCoordinate Coord { get; set; }
        private Brush enabled { get; set; }
        private Brush disabled { get; set; }
        private Brush disc { get; set; }
        private Brush background { get; set; }
        private bool Empty { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="coordinate">Coordinate of cell.</param>
        public RobothelloCellButton(RobothelloCoordinate coordinate)
        {
            Coord = coordinate;
            Style = FindResource("GameCell") as Style;
            MouseEnter += MouseEnters;
            MouseLeave += MouseLeaves;
            enabled = Brushes.ForestGreen;
            disabled = Brushes.DarkGreen;
            disc = Brushes.Transparent;
            background = Brushes.ForestGreen;
            Empty = true;
        }

        /// <summary>
        /// Override of button object method that draws the content of the drawing context object.
        /// Purpose of override is to obtain actual dimention of the panel after it has been rendered.
        /// Disc is uniformly scaled within the area defined by width and height of button.
        /// </summary>
        /// <param name="dc">Drawing context.</param>
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(background, null, new Rect(new Size(ActualWidth, ActualHeight)));
            if (!Empty || disc == disabled)
                dc.DrawEllipse(disc, null, new Point(ActualWidth / 2, ActualHeight / 2), ActualWidth / 3, ActualHeight / 3);
        }

        /// <summary>
        /// Sets the cell owner.
        /// </summary>
        /// <param name="owner">Player owner.</param>
        public void SetOwner(Player owner)
        {
            switch (owner)
            {
                case Player.Empty:
                    disc = enabled;
                    Empty = true;
                    break;
                case Player.AI:
                    disc = Brushes.White;
                    Empty = false;
                    break;
                case Player.Human:
                    disc = Brushes.Black;
                    Empty = false;
                    break;
                case Player.Pending:
                    disc = disabled;
                    Empty = true;
                    break;
            }
            InvalidateVisual();
        }

        /// <summary>
        /// Sets cell enabled.
        /// </summary>
        /// <param name="boolean">True enabled, false disabled.</param>
        public void SetEnabled(bool boolean)
        {
            if (boolean)
            {
                background = enabled;
            }
            else
            {
                background = disabled;
            }
            IsEnabled = boolean;
            InvalidateVisual();
        }

        /// <summary>
        /// Detects when mouse enters cell and displays pending ownership by player (shadow disc).
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void MouseEnters(object sender, MouseEventArgs e)
        {
            if (Empty && IsEnabled)
            {
                SetOwner(Player.Pending);
            }
        }

        /// <summary>
        /// Detects when mouse leaves cell and resets cell as empty is not owned by any player.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs.</param>
        private void MouseLeaves(object sender, MouseEventArgs e)
        {
            if (Empty && IsEnabled)
            {
                SetOwner(Player.Empty);
            }
        }

        /// <summary>
        /// Returns current coordinate of cell.
        /// </summary>
        /// <returns></returns>
        public RobothelloCoordinate GetCoord()
        {
            return Coord;
        }
    }
}
