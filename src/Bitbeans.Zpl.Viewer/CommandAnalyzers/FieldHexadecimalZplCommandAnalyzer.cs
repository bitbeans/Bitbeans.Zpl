using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldHexadecimalZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldHexadecimalZplCommandAnalyzer() : base("^FH") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            char indicator = '_';
            string[] zplDataParts = this.SplitCommand(zplCommand);
            if (zplDataParts[0].Length > 0)
            {
                indicator = zplDataParts[0][0];
            }

            virtualPrinter.SetNextElementFieldHexadecimalIndicator(indicator);

            return null;
        }
    }
}
