using Bitbeans.Zpl.Elements;

using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class DownloadFormatCommandAnalyzer : ZplCommandAnalyzerBase
    {
        private static readonly Regex commandRegex = new(@"^\^DF(\w:)?(.*?)?(\..+?)?$", RegexOptions.Compiled);

        public DownloadFormatCommandAnalyzer() : base("^DF") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            // The pattern accepts every ^DF command (storage device and extension are optional)
            Match commandMatch = commandRegex.Match(zplCommand);
            virtualPrinter.SetNextDownloadFormatName(commandMatch.Groups[2].Value);

            return null;
        }
    }
}
