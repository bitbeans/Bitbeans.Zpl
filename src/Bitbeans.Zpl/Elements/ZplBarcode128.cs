using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>
    /// Code 128 Barcode
    /// </summary>
    public class ZplBarcode128 : ZplBarcode
    {
        /// <summary>
        /// Code 128 mode: N (no selected mode), U (UCC case mode), A (automatic) or D (UCC/EAN mode)
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Calculate and print the UCC check digit (mode U)
        /// </summary>
        public bool UccCheckDigit { get; set; }

        /// <summary>
        /// Code 128 Barcode
        /// </summary>
        /// <param name="content"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="height"></param>
        /// <param name="moduleWidth"></param>
        /// <param name="wideBarToNarrowBarWidthRatio"></param>
        /// <param name="fieldOrientation"></param>
        /// <param name="hexadecimalIndicator"></param>
        /// <param name="printInterpretationLine"></param>
        /// <param name="printInterpretationLineAboveCode"></param>
        /// <param name="bottomToTop"></param>
        /// <param name="useDefaultPosition"></param>
        /// <param name="mode"></param>
        /// <param name="uccCheckDigit"></param>
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
            this.Mode = mode;
            this.UccCheckDigit = uccCheckDigit;
        }

        ///<inheritdoc/>
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
            result.Add(RenderModuleWidth());
            result.Add(command);
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
