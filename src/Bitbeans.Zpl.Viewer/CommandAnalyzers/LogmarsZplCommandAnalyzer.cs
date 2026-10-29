using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers;

/// <summary>Analyzes <c>^BL</c> (LOGMARS) and stores the barcode parameters for the next field.</summary>
public class LogmarsZplCommandAnalyzer : ZplCommandAnalyzerBase
{
    /// <summary>Initializes a new instance of the <see cref="LogmarsZplCommandAnalyzer"/> class for the <c>^BL</c> command.</summary>
    public LogmarsZplCommandAnalyzer() : base("^BL")
    {
    }

    ///<inheritdoc/>
    public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
    {
        string[] zplDataParts = this.SplitCommand(zplCommand);

        // ^BLN,100,N
        FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);
        int? height = null;

        if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out int tmpint))
        {
            height = tmpint;
        }

        bool printInterpretationLineAbove = false;

        if (zplDataParts.Length > 2)
        {
            printInterpretationLineAbove = this.ConvertBoolean(zplDataParts[2]);
        }

        virtualPrinter.SetNextElementFieldData(
            new CodeLogmarsBarcodeFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                PrintInterpretationLineAboveCode = printInterpretationLineAbove
            });

        return null;
    }
}
