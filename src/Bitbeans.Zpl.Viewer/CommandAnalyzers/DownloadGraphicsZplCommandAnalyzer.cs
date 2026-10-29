using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.SkiaSharp;
using Bitbeans.Zpl.Viewer.Helpers;

using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>~DG</c> (download graphics), converts the hexadecimal GRF data to a PNG image and stores it in the printer storage.</summary>
    public class DownloadGraphicsZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        private static readonly Regex commandRegex = new(@"^~DG(\w:)?([^,]*),(\d+),(\d+),(.+)$", RegexOptions.Compiled);

        /// <summary>Initializes a new instance of the <see cref="DownloadGraphicsZplCommandAnalyzer"/> class for the <c>~DG</c> command.</summary>
        public DownloadGraphicsZplCommandAnalyzer() : base("~DG") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            Match commandMatch = commandRegex.Match(zplCommand);
            if (commandMatch.Success)
            {
                char storageDevice = commandMatch.Groups[1].Success ? commandMatch.Groups[1].Value[0] : 'R';
                string imageName = GetFileName(commandMatch.Groups[2].Value);
                _ = int.TryParse(commandMatch.Groups[3].Value, out int totalNumberOfBytesInGraphic);
                _ = int.TryParse(commandMatch.Groups[4].Value, out int numberOfBytesPerRow);
                string dataHex = commandMatch.Groups[5].Value;

                byte[] grfImageData = ImageHelper.GetImageBytes(dataHex, numberOfBytesPerRow);

                // assert grfImageData.Length == totalNumberOfBytesInGraphic

                SkiaImageConverter converter = new();
                byte[] imageData = converter.ConvertGrfToPng(grfImageData, numberOfBytesPerRow);

                printerStorage.AddFile(storageDevice, imageName, imageData);
            }

            return null;
        }

        /// <summary>
        /// A <c>~DG</c> graphic is always stored with the extension <c>.GRF</c>; an omitted name is <c>UNKNOWN</c>, like on the printer.
        /// </summary>
        private static string GetFileName(string name)
        {
            int dot = name.IndexOf('.');
            string baseName = dot < 0 ? name : name.Substring(0, dot);
            return (baseName.Length == 0 ? "UNKNOWN" : baseName) + ".GRF";
        }
    }
}
