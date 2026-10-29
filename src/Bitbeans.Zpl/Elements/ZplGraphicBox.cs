using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic box (<c>^GB</c>): a box or line with optional rounded corners.</summary>
    /// <remarks>
    /// Format: <c>^GBw,h,t,c,r</c>
    /// </remarks>
    public class ZplGraphicBox : ZplGraphicElement
    {
        /// <summary>Box width in dots (<c>w</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }
        /// <summary>Box height in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; private set; }

        /// <summary>Degree of corner rounding (<c>r</c> parameter), 0 (no rounding) to 8. Note that the value is scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int CornerRounding { get; private set; }

        /// <summary>Creates a graphic box.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Box width in dots.</param>
        /// <param name="height">Box height in dots.</param>
        /// <param name="borderThickness">Border (line) thickness in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="lineColor">Line color, <see cref="LineColor.Black"/> (<c>B</c>) or <see cref="LineColor.White"/> (<c>W</c>).</param>
        /// <param name="cornerRounding">Degree of corner rounding, 0-8; default 0.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplGraphicBox(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            int cornerRounding = 0,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(positionX, positionY, borderThickness, lineColor, reversePrint, bottomToTop, useDefaultPosition)
        {
            Width = width;
            Height = height;

            CornerRounding = cornerRounding;
        }

        /// <summary>Renders the position and <c>[^FR]^GBw,h,t,c,r^FS</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the box.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO50,50
            //^ GB300,200,10 ^ FS
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"{RenderReversePrint()}^GB{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)},{context.Scale(CornerRounding)}^FS");

            return result;
        }
    }
}
