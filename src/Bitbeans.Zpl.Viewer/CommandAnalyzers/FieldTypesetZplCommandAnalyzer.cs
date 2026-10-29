using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using System.Globalization;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^FT</c> (field typeset) and stores the baseline position (shifted by the label home ^LH) and optional justification of the next field. Missing coordinates select the default position.</summary>
    public class FieldTypesetZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="FieldTypesetZplCommandAnalyzer"/> class for the <c>^FT</c> command.</summary>
        public FieldTypesetZplCommandAnalyzer() : base("^FT") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            decimal tmpdec;
            int x = 0;
            int y = 0;
            bool useDefaultPosition = false;
            bool useDefaultPositionX = false;

            // A missing or invalid x coordinate is the position after the last formatted field
            if (!string.IsNullOrEmpty(zplDataParts[0]) &&
                decimal.TryParse(zplDataParts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
            {
                x = decimal.ToInt32(tmpdec);
            }
            else
            {
                useDefaultPosition = true;
                useDefaultPositionX = true;
            }

            // An omitted y coordinate is the position after the last formatted text field. The drawers cannot
            // continue at y alone, so the y of the last positioned field is used.
            bool yOmitted = zplDataParts.Length < 2 || string.IsNullOrEmpty(zplDataParts[1]);
            bool yValid = false;
            if (!yOmitted)
            {
                if (decimal.TryParse(zplDataParts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out tmpdec) &&
                    int.MinValue <= tmpdec && tmpdec <= int.MaxValue)
                {
                    y = decimal.ToInt32(tmpdec);
                    yValid = true;
                }
                else
                {
                    // Invalid y coordinate - use default position
                    useDefaultPosition = true;
                }
            }

            // Only x omitted (^FT,y): continue at the x position of the previous field at the explicit baseline y.
            // Without a valid y this is the complete default position.
            useDefaultPositionX &= yValid;

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

            virtualPrinter.SetNextElementPosition(x, y, true, useDefaultPosition, useDefaultPositionX);

            return null;
        }
    }
}
