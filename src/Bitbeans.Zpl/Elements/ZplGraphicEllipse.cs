using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic ellipse (<c>^GE</c>).</summary>
    /// <remarks>
    /// Format: <c>^GEw,h,t,c</c>
    /// </remarks>
    public class ZplGraphicEllipse : ZplGraphicElement
    {
        /// <summary>Ellipse width in dots (<c>w</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }
        /// <summary>Ellipse height in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; private set; }

        /// <summary>Creates a graphic ellipse.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Ellipse width in dots.</param>
        /// <param name="height">Ellipse height in dots.</param>
        /// <param name="borderThickness">Border (line) thickness in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="lineColor">Line color, <see cref="LineColor.Black"/> (<c>B</c>) or <see cref="LineColor.White"/> (<c>W</c>).</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplGraphicEllipse(
            int positionX,
            int positionY,
            int width,
            int height,
            int borderThickness = 1,
            LineColor lineColor = LineColor.Black,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(positionX, positionY, borderThickness, lineColor, reversePrint, bottomToTop, useDefaultPosition)
        {
            Width = width;
            Height = height;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ GE300,100,10,B ^ FS
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"{RenderReversePrint()}^GE{context.Scale(Width)},{context.Scale(Height)},{context.Scale(BorderThickness)},{RenderLineColor(LineColor)}^FS");

            return result;
        }
    }
}
