using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^LH</c> (label home) and stores the label home offset that is added to following field positions.</summary>
    public class LabelHomeZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="LabelHomeZplCommandAnalyzer"/> class for the <c>^LH</c> command.</summary>
        public LabelHomeZplCommandAnalyzer() : base("^LH") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            int x = 0;
            int y = 0;

            if (int.TryParse(zplDataParts[0], out tmpint))
            {
                x = tmpint;
            }

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                y = tmpint;
            }

            virtualPrinter.SetLabelHome(x, y);

            return null;
        }
    }
}
