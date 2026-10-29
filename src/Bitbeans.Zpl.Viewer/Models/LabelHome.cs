namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The label home position set with <c>^LH</c>.</summary>
    public class LabelHome
    {
        /// <summary>Gets the X offset in dots.</summary>
        public int X { get; private set; }
        /// <summary>Gets the Y offset in dots.</summary>
        public int Y { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="LabelHome"/> class.</summary>
        /// <param name="x">The X offset in dots.</param>
        /// <param name="y">The Y offset in dots.</param>
        public LabelHome(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
