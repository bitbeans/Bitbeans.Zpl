using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class CommentZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
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
