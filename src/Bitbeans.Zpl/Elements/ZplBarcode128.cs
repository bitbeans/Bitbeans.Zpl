using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Code 128 bar code (<c>^BC</c>), rendered together with <c>^BY</c> and the field position.</summary>
    public class ZplBarcode128 : ZplBarcode
    {
        /// <summary>Code 128 mode (<c>m</c> parameter of <c>^BC</c>): <c>N</c> (no selected mode, default), <c>U</c> (UCC case mode), <c>A</c> (automatic mode) or <c>D</c> (UCC/EAN mode). Other values are rejected.</summary>
        /// <exception cref="ArgumentException">The value is not <c>N</c>, <c>U</c>, <c>A</c> or <c>D</c>.</exception>
        public string Mode
        {
            get => _mode;
            set => _mode = ValidateMode(value, nameof(value));
        }

        private string _mode;

        /// <summary>Calculate and print the UCC check digit (<c>e</c> parameter of <c>^BC</c>, only used in mode <c>U</c>).</summary>
        public bool UccCheckDigit { get; set; }

        /// <summary>Creates a Code 128 bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
        /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="printInterpretationLine"><see langword="true"/> to print the human-readable interpretation line (<c>Y</c>), <see langword="false"/> to suppress it (<c>N</c>).</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="mode">Code 128 mode: <c>N</c> (default), <c>U</c>, <c>A</c> or <c>D</c>. The <c>e</c> and <c>m</c> parameters of <c>^BC</c> are only rendered when this is not <c>N</c> or <paramref name="uccCheckDigit"/> is set.</param>
        /// <param name="uccCheckDigit">Calculate and print the UCC check digit (mode <c>U</c>); default <see langword="false"/>.</param>
        public ZplBarcode128(
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
            bool useDefaultPosition = false,
            string mode = "N",
            bool uccCheckDigit = false)
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
            _mode = ValidateMode(mode, nameof(mode));
            this.UccCheckDigit = uccCheckDigit;
        }

        private static string ValidateMode(string mode, string paramName)
        {
            if (mode is not ("N" or "U" or "A" or "D"))
            {
                throw new ArgumentException("The Code 128 mode must be N, U, A or D.", paramName);
            }

            return mode;
        }

        /// <summary>Renders <c>^FO</c>/<c>^FT</c>, <c>^BY</c>, <c>^BCo,h,f,g[,e,m]</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^BCo,h,f,g,e,m
            var command = $"^BC{RenderFieldOrientation()},{context.Scale(Height)},{RenderPrintInterpretationLine()},{RenderPrintInterpretationLineAboveCode()}";
            if (UccCheckDigit || Mode != "N")
            {
                command += $",{RenderBoolean(UccCheckDigit)},{Mode}";
            }

            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add(RenderModuleWidth(context));
            result.Add(command);
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
