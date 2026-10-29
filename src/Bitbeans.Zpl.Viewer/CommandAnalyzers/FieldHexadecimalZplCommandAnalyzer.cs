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
            // The parameter is read untrimmed because a space is a valid indicator.
            // ^, ~, control and non-ASCII characters cannot be indicators, the default is used instead.
            string parameter = zplCommand.Substring(this.PrinterCommandPrefix.Length);
            if (parameter.Length > 0 && parameter[0] is >= ' ' and < '~' and not '^')
            {
                indicator = parameter[0];
            }

            virtualPrinter.SetNextElementFieldHexadecimalIndicator(indicator);

            return null;
        }
    }
}
