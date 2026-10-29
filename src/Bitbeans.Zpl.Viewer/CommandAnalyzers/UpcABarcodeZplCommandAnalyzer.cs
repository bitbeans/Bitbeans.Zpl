using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BU</c> (UPC-A) and stores the barcode parameters for the next field.</summary>
    public class UpcABarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="UpcABarcodeZplCommandAnalyzer"/> class for the <c>^BU</c> command.</summary>
        public UpcABarcodeZplCommandAnalyzer() : base("^BU") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = false;
            bool printCheckDigit = true;

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);
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
                printCheckDigit = this.ConvertBoolean(zplDataParts[4]);
            }

            virtualPrinter.SetNextElementFieldData(new UpcABarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode,
                PrintCheckDigit = printCheckDigit
            });

            return null;
        }
    }
}
