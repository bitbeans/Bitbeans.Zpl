using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Field typeset (<c>^FT</c>). Sets the field position relative to the label home (<c>^LH</c>). The typesetting origin is fixed with respect to the field contents and does not change with rotation; for text it is the baseline start, for bar codes the lower-left corner.</summary>
    public class ZplFieldTypeset : ZplElementBase
    {
        /// <summary>X position in dots (0 to 32000), relative to the label home. Scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int PositionX { get; protected set; }
        /// <summary>Y position in dots (0 to 32000), relative to the label home. Scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int PositionY { get; protected set; }
        /// <summary>When <see langword="true"/> no coordinates are rendered, so the printer continues at the position that follows the previous field.</summary>
        public bool UseDefaultPosition { get; private set; }
        /// <summary>Field justification (<c>z</c> parameter of <c>^FT</c>); <see cref="FieldJustification.None"/> omits it.</summary>
        public FieldJustification FieldJustification { get; protected set; }

        /// <summary>Creates a field typeset.</summary>
        /// <param name="positionX">X position in dots (0-32000).</param>
        /// <param name="positionY">Y position in dots (0-32000).</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c>; <see cref="FieldJustification.None"/> omits it.</param>
        /// <param name="useDefaultPosition"><see langword="true"/> to render <c>^FT</c> without coordinates (<c>^FT</c> or <c>^FT,,z</c>).</param>
        public ZplFieldTypeset(int positionX, int positionY, FieldJustification fieldJustification = FieldJustification.None, bool useDefaultPosition = false)
        {
            this.PositionX = positionX;
            this.PositionY = positionY;
            this.FieldJustification = fieldJustification;
            this.UseDefaultPosition = useDefaultPosition;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            // When UseDefaultPosition is true, we don't output coordinates in ^FT command
            if (UseDefaultPosition)
            {
                if (FieldJustification != FieldJustification.None)
                {
                    return new string[] { $"^FT,,{RenderFieldJustification(this.FieldJustification)}" };
                }
                return new string[] { "^FT" };
            }

            // Normal rendering with coordinates
            return new string[] { $"^FT{context.Scale(this.PositionX)},{context.Scale(this.PositionY)},{RenderFieldJustification(this.FieldJustification)}".TrimEnd(',') };
        }

        /// <summary>Returns a new instance moved by the given offset.</summary>
        /// <param name="offsetX">Offset on the x axis in dots.</param>
        /// <param name="offsetY">Offset on the y axis in dots.</param>
        /// <returns>A new <see cref="ZplFieldTypeset"/>; this instance is not changed.</returns>
        public ZplFieldTypeset Offset(int offsetX, int offsetY)
        {
            return new ZplFieldTypeset(this.PositionX + offsetX, this.PositionY + offsetY, this.FieldJustification, this.UseDefaultPosition);
        }
    }
}
