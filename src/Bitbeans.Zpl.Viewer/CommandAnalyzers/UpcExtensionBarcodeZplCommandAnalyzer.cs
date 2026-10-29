using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BS</c> (UPC/EAN extension) and stores the barcode parameters for the next field.</summary>
    public class UpcExtensionBarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="UpcExtensionBarcodeZplCommandAnalyzer"/> class for the <c>^BS</c> command.</summary>
        public UpcExtensionBarcodeZplCommandAnalyzer() : base("^BS") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = true;

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
                printInterpretationLineAboveCode = this.ConvertBoolean(zplDataParts[3], "Y");
            }

            virtualPrinter.SetNextElementFieldData(new UpcExtensionBarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode
            });

            return null;
        }
    }
}
