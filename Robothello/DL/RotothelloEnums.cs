/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-17
/// Modified: -
/// ---------------------------

namespace Robothello.DL
{
    /// <summary>
    /// Player types.
    /// </summary>
    public enum Player : int
    {
        Human = -1,
        Empty = 0,
        AI = 1,
        Pending = 2
    }

    /// <summary>
    /// Grid (board) sizes.
    /// </summary>
    public enum GridSize : int
    {
        Small = 4,
        Standard = 8,
        Large = 12,
        XL = 16,
    }
}
