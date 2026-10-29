namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of the graphic commands (<c>^GB</c>, <c>^GC</c>, <c>^GD</c>, <c>^GE</c>) that share position, border thickness, line color and reverse print.</summary>
    public abstract class ZplGraphicElement : ZplPositionedElementBase
    {
        /// <summary>Line color (<c>B</c> black or <c>W</c> white).</summary>
        public LineColor LineColor { get; protected set; }

        /// <summary>Border thickness in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int BorderThickness { get; protected set; }

        /// <summary>Print the element in reverse (<c>^FR</c>), inverting the underlying image.</summary>
        public bool ReversePrint { get; protected set; }

        /// <summary>Initializes the graphic element.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="borderThickness">Border thickness in dots; default 1.</param>
        /// <param name="lineColor">Line color, <see cref="LineColor.Black"/> (<c>B</c>) or <see cref="LineColor.White"/> (<c>W</c>).</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        public ZplGraphicElement(
            int positionX,
            int positionY,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
        {
            BorderThickness = borderThickness;
            LineColor = lineColor;
            ReversePrint = reversePrint;
        }

        /// <summary>
        /// Render the field reverse print command (^FR) if <see cref="ReversePrint"/> is set
        /// </summary>
        /// <returns>^FR or an empty string</returns>
        protected string RenderReversePrint()
        {
            return ReversePrint ? "^FR" : string.Empty;
        }
    }
}
