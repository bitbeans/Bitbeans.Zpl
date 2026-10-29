using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BC</c> (Code 128) and stores the barcode parameters for the next field.</summary>
    public class Code128BarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="Code128BarcodeZplCommandAnalyzer"/> class for the <c>^BC</c> command.</summary>
        public Code128BarcodeZplCommandAnalyzer() : base("^BC") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            int tmpint;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = false;
            bool uccCheckDigit = false;
            string mode = "N";

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                height = tmpint;
            }

            if (zplDataParts.Length > 2)
            {
                printInterpretationLine = this.ConvertBoolean(zplDataParts[2], "Y");
            }

            if (zplDataParts.Length > 3)
            {
                printInterpretationLineAboveCode = this.ConvertBoolean(zplDataParts[3]);
            }

            if (zplDataParts.Length > 4)
            {
                uccCheckDigit = this.ConvertBoolean(zplDataParts[4]);
            }

            // Unknown modes are printed like the normal mode
            if (zplDataParts.Length > 5)
            {
                string parsedMode = zplDataParts[5].ToUpperInvariant();
                if (parsedMode is "U" or "A" or "D")
                {
                    mode = parsedMode;
                }
            }

            //The field data are processing in the FieldDataZplCommandAnalyzer
            virtualPrinter.SetNextElementFieldData(new Code128BarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode,
                UccCheckDigit = uccCheckDigit,
                Mode = mode
            });

            return null;
        }
    }
}
