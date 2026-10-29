using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>
    /// Graphic Circle<br/>
    /// The ^GC command produces a circle on the printed label.
    /// The command parameters specify the diameter(width) of the circle, outline thickness, and color.
    /// Thickness extends inward from the outline.
    /// </summary>
    /// <remarks>
    /// Format:^GCd,t,c
    /// d = circle diameter
    /// t = border thickness
    /// c = line color
    /// </remarks>
    public class ZplGraphicCircle : ZplGraphicElement
    {
        /// <summary>Circle diameter in dots (<c>d</c> parameter, 3 to 4095); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Diameter { get; private set; }

        /// <summary>Creates a graphic circle.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="diameter">Circle diameter in dots.</param>
        /// <param name="borderThickness">Border (line) thickness in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="lineColor">Line color, <see cref="LineColor.Black"/> (<c>B</c>) or <see cref="LineColor.White"/> (<c>W</c>).</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        public ZplGraphicCircle(
            int positionX,
            int positionY,
            int diameter,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, borderThickness, lineColor, reversePrint, bottomToTop, useDefaultPosition, fieldJustification)
        {
            Diameter = diameter;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GCd,t,c
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"{RenderReversePrint()}^GC{context.Scale(Diameter)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)}^FS");

            return result;
        }
    }
}
