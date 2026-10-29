using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic diagonal line (<c>^GD</c>), drawn across a box of the given width and height.</summary>
    /// <remarks>
    /// Format: <c>^GDw,h,t,c,o</c>
    /// </remarks>
    public class ZplGraphicDiagonalLine : ZplGraphicElement
    {
        /// <summary>Width of the box containing the line in dots (<c>w</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }
        /// <summary>Height of the box containing the line in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; private set; }

        /// <summary><see langword="true"/> for a right-leaning diagonal (<c>R</c>, from bottom left to top right), <see langword="false"/> for a left-leaning one (<c>L</c>, from top left to bottom right).</summary>
        public bool RightLeaningDiagonal { get; private set; }

        /// <summary>Creates a graphic diagonal line.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Width of the box containing the line in dots.</param>
        /// <param name="height">Height of the box containing the line in dots.</param>
        /// <param name="borderThickness">Border (line) thickness in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="lineColor">Line color, <see cref="LineColor.Black"/> (<c>B</c>) or <see cref="LineColor.White"/> (<c>W</c>).</param>
        /// <param name="rightLeaningDiagonal"><see langword="true"/> (default) for a right-leaning diagonal, <see langword="false"/> for a left-leaning one.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        public ZplGraphicDiagonalLine(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            bool rightLeaningDiagonal = true,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, borderThickness, lineColor, reversePrint, bottomToTop, useDefaultPosition, fieldJustification)
        {
            Width = width;
            Height = height;
            
            RightLeaningDiagonal = rightLeaningDiagonal;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GDw,h,t,c,o
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"{RenderReversePrint()}^GD{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)},{(RightLeaningDiagonal ? "R" : "L")}^FS");

            return result;
        }
    }
}
