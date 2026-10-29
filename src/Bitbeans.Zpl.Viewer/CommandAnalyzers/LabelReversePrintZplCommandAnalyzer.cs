using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^LR</c> (label reverse print) and stores whether all following fields are printed in reverse.</summary>
    public class LabelReversePrintZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="LabelReversePrintZplCommandAnalyzer"/> class for the <c>^LR</c> command.</summary>
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
