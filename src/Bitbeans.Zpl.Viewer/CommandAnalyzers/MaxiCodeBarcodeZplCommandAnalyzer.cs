using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^BD</c> (MaxiCode) and stores the barcode parameters for the next field.</summary>
    public class MaxiCodeBarcodeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="MaxiCodeBarcodeZplCommandAnalyzer"/> class for the <c>^BD</c> command.</summary>
        public MaxiCodeBarcodeZplCommandAnalyzer() : base("^BD") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int mode = 2;
            int position = 1;
            int total = 1;

            if (zplDataParts[0] != "")
            {
                mode = int.Parse(zplDataParts[0]);
            }

            int tmpint;
            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                position = tmpint;
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                total = tmpint;
            }

            virtualPrinter.SetNextElementFieldData(new MaxiCodeBarcodeFieldData
            {
                Mode = mode,
                Position = position,
                Total = total,
            });

            return null;
        }
    }
}
