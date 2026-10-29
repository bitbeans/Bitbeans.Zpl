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

        /// <summary>Initializes a new instance of the <see cref="LabelPosition"/> class.</summary>
        /// <param name="x">The X position in dots.</param>
        /// <param name="y">The Y position in dots.</param>
        /// <param name="calculateFromBottom"><see langword="true"/> for <c>^FT</c>.</param>
        /// <param name="useDefaultPosition"><see langword="true"/> to continue at the default position.</param>
        public LabelPosition(int x, int y, bool calculateFromBottom, bool useDefaultPosition)
        {
            this.X = x;
            this.Y = y;
            this.CalculateFromBottom = calculateFromBottom;
            this.UseDefaultPosition = useDefaultPosition;
        }
    }
}
