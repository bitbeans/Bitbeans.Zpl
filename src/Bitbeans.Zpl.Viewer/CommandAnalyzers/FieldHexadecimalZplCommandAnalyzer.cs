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
            // ^, ~, spaces and control characters cannot be indicators, the default is used instead
            if (zplDataParts[0].Length > 0 && zplDataParts[0][0] is > ' ' and < '~' and not '^')
            {
                indicator = zplDataParts[0][0];
            }

            virtualPrinter.SetNextElementFieldHexadecimalIndicator(indicator);

            return null;
        }
    }
}
