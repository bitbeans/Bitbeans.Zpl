using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^A</c> (scalable/bitmapped font) and stores the font name, orientation, height and width for the next field only.</summary>
    public class ScalableBitmappedFontZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="ScalableBitmappedFontZplCommandAnalyzer"/> class for the <c>^A</c> command.</summary>
        public ScalableBitmappedFontZplCommandAnalyzer() : base("^A") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            // Font names are A-Z and 0-9; other names (e.g. ^A@ with a font file) keep the current font
            string fontName = zplCommand.Length > this.PrinterCommandPrefix.Length
                && char.IsAsciiLetterOrDigit(zplCommand[this.PrinterCommandPrefix.Length])
                ? char.ToUpperInvariant(zplCommand[this.PrinterCommandPrefix.Length]).ToString()
                : virtualPrinter.NextFont?.FontName ?? virtualPrinter.FontName;

            string[] zplDataParts = this.SplitCommand(zplCommand, 1);

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            int tmpint;
            int? parsedHeight = null;
            int? parsedWidth = null;

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                parsedHeight = tmpint;
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                parsedWidth = tmpint;
            }

            int fontHeight = parsedHeight ?? virtualPrinter.FontHeight;
            int fontWidth = parsedWidth ?? virtualPrinter.FontWidth;

            // Without an explicit orientation the font follows the default orientation (^FW), even if ^FW comes later
            bool orientationIsExplicit = zplDataParts[0] is "N" or "R" or "I" or "B";

            virtualPrinter.SetNextFont(fontName, fieldOrientation, fontWidth, fontHeight, orientationIsExplicit);

            return null;
        }
    }
}
