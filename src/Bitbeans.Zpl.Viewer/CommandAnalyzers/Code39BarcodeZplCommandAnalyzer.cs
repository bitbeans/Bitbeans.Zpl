using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^B3</c> (Code 39) and stores the barcode parameters for the next field.</summary>
    public class Code39BarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="Code39BarcodeZplCommandAnalyzer"/> class for the <c>^B3</c> command.</summary>
        public Code39BarcodeZplCommandAnalyzer() : base("^B3") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            bool mod43CheckDigit = false;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = false;

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);
            if (zplDataParts.Length > 1)
            {
                mod43CheckDigit = this.ConvertBoolean(zplDataParts[1]);
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                height = tmpint;
            }

            if (zplDataParts.Length > 3)
            {
                printInterpretationLine = this.ConvertBoolean(zplDataParts[3], "Y");
            }

            if (zplDataParts.Length > 4)
            {
                printInterpretationLineAboveCode = this.ConvertBoolean(zplDataParts[4]);
            }

            //The field data are processing in the FieldDataZplCommandAnalyzer
            virtualPrinter.SetNextElementFieldData(new Code39BarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode,
                Mod43CheckDigit = mod43CheckDigit
            });

            return null;
        }
    }
}
