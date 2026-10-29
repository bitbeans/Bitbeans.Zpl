using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers;

public class LogmarsZplCommandAnalyzer : ZplCommandAnalyzerBase
{
    public LogmarsZplCommandAnalyzer() : base("^BL")
    {
    }

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
            printInterpretationLineAbove = this.ConvertBoolean(zplDataParts[2], "Y");
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
