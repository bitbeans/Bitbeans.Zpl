using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldReversePrintZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldReversePrintZplCommandAnalyzer() : base("^FR") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            virtualPrinter.SetNextElementFieldReverse();

            return null;
        }
    }
}
