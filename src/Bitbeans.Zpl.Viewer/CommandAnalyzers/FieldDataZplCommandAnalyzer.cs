using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldDataZplCommandAnalyzer : ZplCommandAnalyzerBase
    {

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
