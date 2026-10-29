using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^GS</c> (graphic symbol) and stores the orientation, height and width for the next field, whose field data selects the symbol character.</summary>
    public class GraphicSymbolZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="GraphicSymbolZplCommandAnalyzer"/> class for the <c>^GS</c> command.</summary>
        public GraphicSymbolZplCommandAnalyzer() : base("^GS") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            FieldOrientation fieldOrientation = this.ConvertFieldOrientation(zplDataParts[0], virtualPrinter);

            int tmpint;
            int height = virtualPrinter.FontHeight;
            int width = virtualPrinter.FontWidth;

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                height = tmpint;
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                width = tmpint;
            }

            //The field data are processing in the FieldDataZplCommandAnalyzer
            virtualPrinter.SetNextElementFieldData(new GraphicSymbolFieldData
            {
                FieldOrientation = fieldOrientation,
                Height = height,
                Width = width
            });

            return null;
        }
    }
}
