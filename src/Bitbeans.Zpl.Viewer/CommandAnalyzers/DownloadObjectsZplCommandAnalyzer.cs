using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>~DY</c> (download objects) and stores the hexadecimal object data in the printer storage. The object is stored under its name and extension (<c>LOGO.PNG</c>); downloaded .GRF data is not converted.</summary>
    public class DownloadObjectsZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        /// <summary>Initializes a new instance of the <see cref="DownloadObjectsZplCommandAnalyzer"/> class for the <c>~DY</c> command.</summary>
        public DownloadObjectsZplCommandAnalyzer() : base("~DY") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            char storageDevice = zplCommand[this.PrinterCommandPrefix.Length];

            string[] zplDataParts = this.SplitCommand(zplCommand, 2);

            string objectName = zplDataParts[0];
            // zplDataParts[1] is the format of the data field (b); the bytes are stored as they are
            string extensionOfStoredFile = zplDataParts[2];
            _ = int.TryParse(zplDataParts[3], out int objectDataLength);
            _ = int.TryParse(zplDataParts[4], out int totalNumberOfBytesPerRow);

            // TODO: Handle case when .GRF data is downloaded using the ~DY command
            string dataHex = zplDataParts[5];

            printerStorage.AddFile(storageDevice, GetFileName(objectName, extensionOfStoredFile), ByteHelper.HexToBytes(dataHex));

            return null;
        }

        /// <summary>
        /// The printer stores the object under its name and the extension given by the <c>x</c> parameter
        /// (<c>~DYR:LOGO,P,P,...</c> becomes <c>LOGO.PNG</c>), so <c>^IM</c> and <c>^XG</c> find it by its full name.
        /// A name that already has an extension is kept; an omitted name is <c>UNKNOWN</c> and an unknown extension code <c>.GRF</c>, like on the printer.
        /// </summary>
        private static string GetFileName(string objectName, string extensionCode)
        {
            if (objectName.Length == 0 || objectName[0] == '.')
            {
                objectName = "UNKNOWN" + objectName;
            }

            if (objectName.Contains('.'))
            {
                return objectName;
            }

            string extension = extensionCode.Trim().ToUpperInvariant() switch
            {
                "B" => "BMP",
                "C" => "WML",
                "E" => "TTE",
                "F" => "HTM",
                "H" => "GET",
                "NRD" => "NRD",
                "P" => "PNG",
                "PAC" => "PAC",
                "T" => "TTF",
                "X" => "PCX",
                // G and every other value: raw bitmap, the printer's default
                _ => "GRF",
            };

            return objectName + "." + extension;
        }
    }
}
