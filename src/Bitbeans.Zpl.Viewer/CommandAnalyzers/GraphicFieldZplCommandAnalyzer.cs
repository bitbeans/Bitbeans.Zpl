using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Helpers;

using System;
using System.IO;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^GF</c> (graphic field), validates the graphic data and returns a graphic field element with the unchanged graphic data at the pending field position. The element converts the data to an image when it is drawn.</summary>
    public class GraphicFieldZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="GraphicFieldZplCommandAnalyzer"/> class for the <c>^GF</c> command.</summary>
        public GraphicFieldZplCommandAnalyzer() : base("^GF") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            int tmpint;
            int binaryByteCount = 0;
            int graphicFieldCount = 0;
            int bytesPerRow = 0;

            int x = 0;
            int y = 0;
            bool bottomToTop = false;
            bool useDefaultPosition = false;

            if (virtualPrinter.NextElementPosition != null)
            {
                x = virtualPrinter.NextElementPosition.X;
                y = virtualPrinter.NextElementPosition.Y;

                bottomToTop = virtualPrinter.NextElementPosition.CalculateFromBottom;
                useDefaultPosition = virtualPrinter.NextElementPosition.UseDefaultPosition;
            }

            string[] zplDataParts = this.SplitCommand(zplCommand);

            char compressionType = zplDataParts[0][0];

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                binaryByteCount = tmpint;
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                graphicFieldCount = tmpint;
            }

            if (zplDataParts.Length > 3 && int.TryParse(zplDataParts[3], out tmpint))
            {
                bytesPerRow = tmpint;
            }

            //fourth comma is the start of the image data
            int indexOfFourthComma = this.IndexOfNthCharacter(zplCommand, 4, ',');
            string dataHex = zplCommand.Substring(indexOfFourthComma + 1).TrimStart();

            // Reject data that cannot be drawn so that the problem is reported while analyzing
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytesPerRow);
            if (ImageHelper.GetImageBytes(dataHex, bytesPerRow).Length < bytesPerRow)
            {
                throw new InvalidDataException("Graphic data is smaller than one row.");
            }

            return new ZplGraphicField(x, y, binaryByteCount, graphicFieldCount, bytesPerRow, dataHex, bottomToTop, useDefaultPosition, compressionType, ResolveFieldJustification(virtualPrinter));
        }
    }
}
