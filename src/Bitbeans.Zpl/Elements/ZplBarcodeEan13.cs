using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>EAN-13 bar code (<c>^BE</c>), rendered together with <c>^BY</c> and the field position.</summary>
    public class ZplBarcodeEan13 : ZplBarcode
    {
        /// <summary>Creates an EAN-13 bar code.</summary>
        /// <param name="content">The 12 digits to encode; the printer calculates the 13th (check) digit.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
        /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="printInterpretationLine"><see langword="true"/> to print the human-readable interpretation line (<c>Y</c>), <see langword="false"/> to suppress it (<c>N</c>).</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplBarcodeEan13(
            string content,
            int positionX,
            int positionY,
            int height = 100,
            int moduleWidth = 2,
            double wideBarToNarrowBarWidthRatio = 3,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool printInterpretationLine = true,
            bool printInterpretationLineAboveCode = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(content,
                  positionX,
                  positionY,
                  height,
                  moduleWidth,
                  wideBarToNarrowBarWidthRatio,
                  fieldOrientation,
                  hexadecimalIndicator,
                  printInterpretationLine,
                  printInterpretationLineAboveCode,
                  bottomToTop,
                  useDefaultPosition)
        {
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add(RenderModuleWidth());
            result.Add($"^BE{RenderFieldOrientation()},{context.Scale(Height)},{RenderPrintInterpretationLine()},{RenderPrintInterpretationLineAboveCode()}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
