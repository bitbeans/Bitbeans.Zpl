using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>~DY</c> (download objects) and stores the hexadecimal object data in the printer storage. Downloaded .GRF data is not converted.</summary>
    public class DownloadObjectsZplCommandAnaylzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="DownloadObjectsZplCommandAnaylzer"/> class for the <c>~DY</c> command.</summary>
        public DownloadObjectsZplCommandAnaylzer() : base("~DY") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            char storageDevice = zplCommand[this.PrinterCommandPrefix.Length];

            string[] zplDataParts = this.SplitCommand(zplCommand, 2);

            string objectName = zplDataParts[0];
            string formatDownloadedInDataField = zplDataParts[1];
            string extensionOfStoredFile = zplDataParts[2];
            _ = int.TryParse(zplDataParts[3], out int objectDataLength);
            _ = int.TryParse(zplDataParts[4], out int totalNumberOfBytesPerRow);

            // TODO: Handle case when .GRF data is downloaded using the ~DY command
            string dataHex = zplDataParts[5];

            printerStorage.AddFile(storageDevice, objectName, ByteHelper.HexToBytes(dataHex));

            return null;
        }
    }
}
