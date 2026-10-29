using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FX</c> (comment) and records the comment text in the virtual printer; no element is created.</summary>
    public class CommentZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="CommentZplCommandAnalyzer"/> class for the <c>^FX</c> command.</summary>
        public CommentZplCommandAnalyzer() : base("^FX") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string comment = zplCommand.Substring(this.PrinterCommandPrefix.Length);
            virtualPrinter.AddComment(comment);

            return null;
        }
    }
}
