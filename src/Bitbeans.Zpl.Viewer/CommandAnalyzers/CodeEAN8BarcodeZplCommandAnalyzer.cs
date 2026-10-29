using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^B8</c> (EAN-8) and stores the barcode parameters for the next field.</summary>
    public class CodeEAN8BarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="CodeEAN8BarcodeZplCommandAnalyzer"/> class for the <c>^B8</c> command.</summary>
        public CodeEAN8BarcodeZplCommandAnalyzer() : base("^B8") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            int tmpint;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = false;

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

            //The field data are processing in the FieldDataZplCommandAnalyzer
            virtualPrinter.SetNextElementFieldData(new CodeEAN8BarcodeFieldData
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
