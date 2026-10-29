using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^CF</c> (change alphanumeric default font) and stores the default font name, height and width in the virtual printer. Omitted values keep the previous ones.</summary>
    public class ChangeAlphanumericDefaultFontZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="ChangeAlphanumericDefaultFontZplCommandAnalyzer"/> class for the <c>^CF</c> command.</summary>
        public ChangeAlphanumericDefaultFontZplCommandAnalyzer() : base("^CF") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            // Not supplied (or invalid) values keep the last ^CF dimensions; the initial default height is 9 dots
            int fontHeight = virtualPrinter.FontHeight > 0 ? virtualPrinter.FontHeight : 9;
            int fontWidth = virtualPrinter.FontWidth;

            string newFont = zplDataParts[0];
            if (!string.IsNullOrEmpty(newFont))
            {
                virtualPrinter.SetFontName(newFont);
            }

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                fontHeight = tmpint;
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                fontWidth = tmpint;
            }

            virtualPrinter.SetFontHeight(fontHeight);
            virtualPrinter.SetFontWidth(fontWidth);

            return null;
        }
    }
}
