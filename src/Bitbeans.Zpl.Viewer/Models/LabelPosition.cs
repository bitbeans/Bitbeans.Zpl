namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The position of the next field set with <c>^FO</c> or <c>^FT</c>.</summary>
    public class LabelPosition
    {
        /// <summary>Gets the X position in dots, including the label home.</summary>
        public int X { get; private set; }
        /// <summary>Gets the Y position in dots, including the label home.</summary>
        public int Y { get; private set; }
        /// <summary>Gets a value indicating whether the position is the bottom-left origin of <c>^FT</c> instead of the top-left origin of <c>^FO</c>.</summary>
        public bool CalculateFromBottom { get; private set; }
        /// <summary>Gets a value indicating whether the field continues at the default position (<c>^FT</c> without coordinates).</summary>
        public bool UseDefaultPosition { get; private set; }
        /// <summary>Gets a value indicating whether only the x coordinate was omitted (<c>^FT,y</c>): <see cref="Y"/> is explicit and the field continues at the x position that follows the previous field. <see cref="UseDefaultPosition"/> is also set in this case, so elements that cannot continue at x only use the complete default position.</summary>
        public bool UseDefaultPositionX { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="LabelPosition"/> class.</summary>
        /// <param name="x">The X position in dots.</param>
        /// <param name="y">The Y position in dots.</param>
        /// <param name="calculateFromBottom"><see langword="true"/> for <c>^FT</c>.</param>
        /// <param name="useDefaultPosition"><see langword="true"/> to continue at the default position.</param>
        /// <param name="useDefaultPositionX"><see langword="true"/> if only the x coordinate was omitted while the y coordinate is explicit.</param>
        public LabelPosition(int x, int y, bool calculateFromBottom, bool useDefaultPosition, bool useDefaultPositionX = false)
        {
            this.X = x;
            this.Y = y;
            this.CalculateFromBottom = calculateFromBottom;
            this.UseDefaultPosition = useDefaultPosition;
            this.UseDefaultPositionX = useDefaultPositionX;
        }
    }
}
