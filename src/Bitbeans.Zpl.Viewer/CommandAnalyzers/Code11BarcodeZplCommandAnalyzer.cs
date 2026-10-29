using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers;

/// <summary>Analyzes <c>^B1</c> (Code 11) and stores the barcode parameters for the next field.</summary>
public class Code11BarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
{
    /// <summary>Initializes a new instance of the <see cref="Code11BarcodeZplCommandAnalyzer"/> class for the <c>^B1</c> command.</summary>
    public Code11BarcodeZplCommandAnalyzer() : base("^B1")
    {
    }

    ///<inheritdoc/>
    public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
    {
        string[] zplDataParts = this.SplitCommand(zplCommand);

        FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);
        Code11CheckDigitCount checkDigitCount = Code11CheckDigitCount.Two;

        if (zplDataParts.Length > 1)
        {
            checkDigitCount = this.ConvertCode11CheckDigitCount(zplDataParts[1]);
        }

        int? height = null;

        if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out int tmpint))
        {
            height = tmpint;
        }

        bool printInterpretationLine = true;

        if (zplDataParts.Length > 3)
        {
            printInterpretationLine = this.ConvertBoolean(zplDataParts[3], "Y");
        }

        bool printInterpretationLineAboveCode = false;

        if (zplDataParts.Length > 4)
        {
            printInterpretationLineAboveCode = this.ConvertBoolean(zplDataParts[4]);
        }

        virtualPrinter.SetNextElementFieldData(
            new Code11BarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                CheckDigitCount = checkDigitCount,
                Height = height,
                PrintInterpretationLine = printInterpretationLine,
                PrintInterpretationLineAboveCode = printInterpretationLineAboveCode
            });

        return null;
    }
}
