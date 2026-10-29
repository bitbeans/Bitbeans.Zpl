using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using System.Globalization;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldTypesetZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldTypesetZplCommandAnalyzer() : base("^FT") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            decimal tmpdec;
            int x = 0;
            int y = 0;
            bool useDefaultPosition = false;

            // Handle missing coordinates - when coordinates are missing, use default positioning
            if (string.IsNullOrEmpty(zplDataParts[0]))
            {
                // No coordinates specified - use default position
                useDefaultPosition = true;
            }
            else
            {
                if (decimal.TryParse(zplDataParts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                    int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
                {
                    x = decimal.ToInt32(tmpdec);
                }
                else
                {
                    // Empty or invalid x coordinate - use default position
                    useDefaultPosition = true;
                }
            }

            // An omitted y coordinate is the position after the last formatted text field. The drawers can only
            // continue at the complete default position (x and y), so the y of the last positioned field is used.
            bool yOmitted = zplDataParts.Length < 2 || string.IsNullOrEmpty(zplDataParts[1]);
            if (!yOmitted)
            {
                if (decimal.TryParse(zplDataParts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                    int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
                {
                    y = decimal.ToInt32(tmpdec);
                }
                else if (!useDefaultPosition)
                {
                    // Invalid y coordinate but x was valid - use default position
                    useDefaultPosition = true;
                }
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

            if (yOmitted)
            {
                // Already includes the label home
                y = virtualPrinter.LastElementPositionY;
            }

            virtualPrinter.SetNextElementPosition(x, y, true, useDefaultPosition);

            return null;
        }
    }
}
