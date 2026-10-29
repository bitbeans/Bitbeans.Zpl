using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FR</c> (field reverse print) and marks the next field to be printed in reverse.</summary>
    public class FieldReversePrintZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="FieldReversePrintZplCommandAnalyzer"/> class for the <c>^FR</c> command.</summary>
        public FieldReversePrintZplCommandAnalyzer() : base("^FR") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            virtualPrinter.SetNextElementFieldReverse();

            return null;
        }
    }
}
