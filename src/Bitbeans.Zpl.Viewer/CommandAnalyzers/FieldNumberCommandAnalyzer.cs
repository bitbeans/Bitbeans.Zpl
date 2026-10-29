using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldNumberCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldNumberCommandAnalyzer() : base("^FN") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            int fieldNumber = 0;

            if (int.TryParse(zplDataParts[0], out tmpint))
            {
                fieldNumber = tmpint;
            }

            virtualPrinter.SetNextFieldNumber(fieldNumber);
            return null;
        }
    }
}
