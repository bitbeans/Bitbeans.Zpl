using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BK</c> (ANSI Codabar) and stores the barcode parameters for the next field.</summary>
    public class AnsiCodabarBarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="AnsiCodabarBarcodeZplCommandAnalyzer"/> class for the <c>^BK</c> command.</summary>
        public AnsiCodabarBarcodeZplCommandAnalyzer() : base("^BK") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            bool checkDigit = false;
            int? height = null;
            bool printInterpretationLine = true;
            bool printInterpretationLineAboveCode = false;
            char startCharacter = 'A';
            char stopCharacter = 'A';

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            if (zplDataParts.Length > 1)
            {
                checkDigit = this.ConvertBoolean(zplDataParts[1]);
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out int tmpint))
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

            if (zplDataParts.Length > 5 && char.TryParse(zplDataParts[5], out char startChar))
            {
                startCharacter = startChar;
            }

            if (zplDataParts.Length > 6 && char.TryParse(zplDataParts[6], out char stopChar))
            {
                stopCharacter = stopChar;
            }

            //The field data are processing in the FieldDataZplCommandAnalyzer
            virtualPrinter.SetNextElementFieldData(new AnsiCodabarFieldData
            {
                FieldOrientation = fieldOrientation,
                StartCharacter = startCharacter,
                StopCharacter = stopCharacter,
                Height = height,
                CheckDigit = checkDigit,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode,
            });

            return null;
        }

    }
}
