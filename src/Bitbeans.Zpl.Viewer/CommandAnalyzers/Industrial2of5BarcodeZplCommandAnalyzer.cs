using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BI</c> (Industrial 2 of 5) and stores the barcode parameters for the next field.</summary>
    public class Industrial2of5BarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="Industrial2of5BarcodeZplCommandAnalyzer"/> class for the <c>^BI</c> command.</summary>
        public Industrial2of5BarcodeZplCommandAnalyzer() : base("^BI") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            int? height = null;
            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out int tmpint))
            {
                height = tmpint;
            }

            bool printInterpretationLine = true;
            if (zplDataParts.Length > 2)
            {
                printInterpretationLine = this.ConvertBoolean(zplDataParts[2], "Y");
            }

            bool printInterpretationLineAboveCode = false;
            if (zplDataParts.Length > 3)
            {
                printInterpretationLineAboveCode = this.ConvertBoolean(zplDataParts[3]);
            }

            virtualPrinter.SetNextElementFieldData(new Industrial2of5BarcodeFieldData
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
