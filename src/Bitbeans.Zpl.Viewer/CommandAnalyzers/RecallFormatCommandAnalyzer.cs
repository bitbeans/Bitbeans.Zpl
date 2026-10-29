using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^XF</c> (recall format) and returns an element that references the stored label format by name.</summary>
    public class RecallFormatCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="RecallFormatCommandAnalyzer"/> class for the <c>^XF</c> command.</summary>
        public RecallFormatCommandAnalyzer() : base("^XF") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string formatName = zplCommand.Substring(this.PrinterCommandPrefix.Length);

            return new ZplRecallFormat(formatName);
        }
    }
}
