using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FW</c> (field orientation) and stores the default field orientation and justification used by following fields.</summary>
    public class FieldOrientationZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="FieldOrientationZplCommandAnalyzer"/> class for the <c>^FW</c> command.</summary>
        public FieldOrientationZplCommandAnalyzer() : base("^FW") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);
            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);
            virtualPrinter.SetFieldOrientation(fieldOrientation);

            if (zplDataParts.Length > 1)
            {
                FieldJustification fieldJustification = this.ConvertFieldJustification(zplDataParts[1], virtualPrinter);
                virtualPrinter.SetFieldJustification(fieldJustification);
            }

            return null;
        }
    }
}
