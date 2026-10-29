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

            // The number may be followed by an optional quoted prompt: ^FN1"Name"
            string numberText = zplDataParts[0];
            int digitCount = 0;
            while (digitCount < numberText.Length && char.IsAsciiDigit(numberText[digitCount]))
            {
                digitCount++;
            }

            // Empty or out of range numbers are field number 0
            _ = int.TryParse(numberText.Substring(0, digitCount), out int fieldNumber);

            virtualPrinter.SetNextFieldNumber(fieldNumber);
            return null;
        }
    }
}
