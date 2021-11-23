/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

namespace Robothello.DL
{
    /// <summary>
    /// Data class for handling a game cell coordinate.
    /// </summary>
    public class RobothelloCoordinate
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        public RobothelloCoordinate(int x, int y)
        {
            X = x;
            Y = y; 
        }

        /// <summary>
        /// Gets and sets X-coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets and sets Y-coordinate.
        /// </summary>
        public int Y { get; set; }
    }
}
