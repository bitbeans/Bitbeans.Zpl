using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FH</c> (field hexadecimal indicator) and stores the escape character used for hexadecimal sequences in the next field data. Defaults to <c>_</c>.</summary>
    public class FieldHexadecimalZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="FieldHexadecimalZplCommandAnalyzer"/> class for the <c>^FH</c> command.</summary>
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
