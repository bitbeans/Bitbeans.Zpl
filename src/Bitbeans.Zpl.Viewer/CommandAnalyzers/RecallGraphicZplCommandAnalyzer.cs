using Bitbeans.Zpl.Elements;

using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^XG</c> (recall graphic) and returns an element that draws the stored graphic with the given magnification at the pending field position.</summary>
    public class RecallGraphicZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        private static readonly Regex commandRegex = new(@"^\^XG(\w:)?([^,]*)(?:,([^,]*))?(?:,([^,]*))?", RegexOptions.Compiled);

        /// <summary>Initializes a new instance of the <see cref="RecallGraphicZplCommandAnalyzer"/> class for the <c>^XG</c> command.</summary>
        public RecallGraphicZplCommandAnalyzer() : base("^XG") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
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

            // Every ^XG command matches: name, device and magnification are optional, further parameters are ignored
            Match commandMatch = commandRegex.Match(zplCommand);
            char storageDevice = commandMatch.Groups[1].Success ? commandMatch.Groups[1].Value[0] : 'R';
            string imageName = commandMatch.Groups[2].Value;
            int magnificationFactorX = ParseMagnificationFactor(commandMatch.Groups[3]);
            int magnificationFactorY = ParseMagnificationFactor(commandMatch.Groups[4]);

            return new ZplRecallGraphic(x, y, storageDevice, imageName, magnificationFactorX, magnificationFactorY, bottomToTop, useDefaultPosition, ResolveFieldJustification(virtualPrinter));
        }

        /// <summary>
        /// A missing, empty or invalid magnification factor keeps the default of 1.
        /// </summary>
        private static int ParseMagnificationFactor(Group group)
        {
            return group.Success && int.TryParse(group.Value, out int factor) ? factor : 1;
        }
    }
}
