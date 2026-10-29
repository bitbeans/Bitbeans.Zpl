using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using System.Globalization;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FO</c> (field origin) and stores the top-left position (shifted by the label home ^LH) and optional justification of the next field.</summary>
    public class FieldOriginZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="FieldOriginZplCommandAnalyzer"/> class for the <c>^FO</c> command.</summary>
        public FieldOriginZplCommandAnalyzer() : base("^FO") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            decimal tmpdec;
            int x = 0;
            int y = 0;

            if (decimal.TryParse(zplDataParts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
            {
                x = decimal.ToInt32(tmpdec);
            }

            if (zplDataParts.Length > 1 &&
                decimal.TryParse(zplDataParts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
            {
                y = decimal.ToInt32(tmpdec);
            }

            if (zplDataParts.Length > 2)
            {
                FieldJustification fieldJustification = this.ConvertFieldJustification(zplDataParts[2], virtualPrinter);
                virtualPrinter.SetNextElementFieldJustification(fieldJustification);
            }

            if (virtualPrinter.LabelHomePosition != null)
            {
                x += virtualPrinter.LabelHomePosition.X;
                y += virtualPrinter.LabelHomePosition.Y;
            }

            virtualPrinter.SetNextElementPosition(x, y);

            return null;
        }
    }
}
