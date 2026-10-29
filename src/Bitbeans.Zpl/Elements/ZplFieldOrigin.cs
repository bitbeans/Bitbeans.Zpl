using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Field origin (<c>^FO</c>). Sets the upper-left corner of the following field area, relative to the label home (<c>^LH</c>), independent of the field rotation.</summary>
    /// <remarks>
    /// Format: <c>^FOx,y,z</c>
    /// </remarks>
    public class ZplFieldOrigin : ZplElementBase
    {
        /// <summary>X position in dots (0 to 32000), relative to the label home. Scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int PositionX { get; protected set; }
        /// <summary>Y position in dots (0 to 32000), relative to the label home. Scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int PositionY { get; protected set; }
        /// <summary>Field justification (<c>z</c> parameter of <c>^FO</c>); <see cref="FieldJustification.None"/> omits it.</summary>
        public FieldJustification FieldJustification { get; protected set; }

        /// <summary>Creates a field origin.</summary>
        /// <param name="positionX">X position in dots (0-32000).</param>
        /// <param name="positionY">Y position in dots (0-32000).</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c>; <see cref="FieldJustification.None"/> omits it.</param>
        public ZplFieldOrigin(int positionX, int positionY, FieldJustification fieldJustification = FieldJustification.None)
        {
            this.PositionX = positionX;
            this.PositionY = positionY;
            this.FieldJustification = fieldJustification;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^FO50,50
            return new string[] { $"^FO{context.Scale(this.PositionX)},{context.Scale(this.PositionY)},{RenderFieldJustification(this.FieldJustification)}".TrimEnd(',') };
        }

        /// <summary>Returns a new instance moved by the given offset.</summary>
        /// <param name="offsetX">Offset on the x axis in dots.</param>
        /// <param name="offsetY">Offset on the y axis in dots.</param>
        /// <returns>A new <see cref="ZplFieldOrigin"/>; this instance is not changed.</returns>
        public ZplFieldOrigin Offset(int offsetX, int offsetY)
        {
            return new ZplFieldOrigin(this.PositionX + offsetX, this.PositionY + offsetY, this.FieldJustification);
        }
    }
}
