using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class LabelReversePrintZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public LabelReversePrintZplCommandAnalyzer() : base("^LR") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            virtualPrinter.SetLabelReverse(this.ConvertBoolean(zplDataParts[0]));
            return null;
        }
    }
}
