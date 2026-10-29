using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>ANSI Codabar bar code (<c>^BK</c>), rendered together with <c>^BY</c> and the field position.</summary>
    public class ZplBarcodeAnsiCodabar : ZplBarcode
    {
        /// <summary>Calculate and print a check digit (<c>e</c> parameter of <c>^BK</c>).</summary>
        public bool CheckDigit { get; private set; }
        /// <summary>Start character (<c>k</c> parameter of <c>^BK</c>): <c>A</c>, <c>B</c>, <c>C</c> or <c>D</c>.</summary>
        public char StartCharacter { get; private set; }
        /// <summary>Stop character (<c>l</c> parameter of <c>^BK</c>): <c>A</c>, <c>B</c>, <c>C</c> or <c>D</c>.</summary>
        public char StopCharacter { get; private set; }

        /// <summary>Creates an ANSI Codabar bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="startCharacter">Start character, one of <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> (case-insensitive, rendered upper case).</param>
        /// <param name="stopCharacter">Stop character, one of <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> (case-insensitive, rendered upper case).</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
        /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="printInterpretationLine"><see langword="true"/> to print the human-readable interpretation line (<c>Y</c>), <see langword="false"/> to suppress it (<c>N</c>).</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it.</param>
        /// <param name="checkDigit">Calculate and print a check digit; default <see langword="false"/>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <exception cref="InvalidOperationException"><paramref name="startCharacter"/> or <paramref name="stopCharacter"/> is not one of A, B, C, D.</exception>
        public ZplBarcodeAnsiCodabar(
            string content,
            char startCharacter,
            char stopCharacter,
            int positionX,
            int positionY,
            int height = 100,
            int moduleWidth = 2,
            double wideBarToNarrowBarWidthRatio = 3,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool printInterpretationLine = true,
            bool printInterpretationLineAboveCode = false,
            bool checkDigit = false,
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
            if (!IsValidCharacter(startCharacter))
            {
                throw new InvalidOperationException("ANSI Codabar start charactor must be one of A, B, C, D");
            }

            if (!IsValidCharacter(stopCharacter))
            {
                throw new InvalidOperationException("ANSI Codabar stop charactor must be one of A, B, C, D");
            }

            CheckDigit = checkDigit;
            StartCharacter = char.ToUpper(startCharacter);
            StopCharacter = char.ToUpper(stopCharacter);
        }

        private bool IsValidCharacter(char character)
        {
            var chars = new[] { 'A', 'B', 'C', 'D' };
            return chars.Contains(char.ToUpperInvariant(character));
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO100,100 ^ BY3
            // ^ BKN,N,150,Y,N,A,A
            //  ^ FD123456 ^ FS
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add(RenderModuleWidth());
            result.Add($"^BK{RenderFieldOrientation()},{RenderBoolean(CheckDigit)},{context.Scale(Height)},{RenderPrintInterpretationLine()},{RenderPrintInterpretationLineAboveCode()},{StartCharacter},{StopCharacter}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
