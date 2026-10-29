using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FD</c> (field data) and stores the text as content of the next field. If a field number (^FN) is pending, a recall element for that field number is returned.</summary>
    public class FieldDataZplCommandAnalyzer : ZplCommandAnalyzerBase
    {

        /// <summary>Initializes a new instance of the <see cref="FieldDataZplCommandAnalyzer"/> class.</summary>
        /// <param name="prefix">The command prefix to handle: <c>^FD</c> by default, <c>^FV</c> for the derived field variable analyzer.</param>
        public FieldDataZplCommandAnalyzer(string prefix = "^FD") : base(prefix) { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string text = zplCommand.Substring(this.PrinterCommandPrefix.Length);

            // If field data follows a field number, a ZplRecallFieldNumber element has to be returned
            int? fieldNumber = virtualPrinter.NextFieldNumber;
            if (fieldNumber != null)
            {
                if (virtualPrinter.HasNextElementPosition)
                {
                    // ^FO/^FT, ^FN and ^FD in the same field: the field is defined and receives its data (and the
                    // data is also provided to all other fields with this number). The field separator finalizes it.
                    virtualPrinter.SetNextElementFieldDataContent(text);
                }
                else
                {
                    // Data only (recall of a format): prevents consumption by field separator analyzer
                    virtualPrinter.ClearNextFieldNumber();
                }

                return new ZplRecallFieldNumber(fieldNumber.Value, text);
            }

            virtualPrinter.SetNextElementFieldDataContent(text);

            return null;
        }
    }
}
