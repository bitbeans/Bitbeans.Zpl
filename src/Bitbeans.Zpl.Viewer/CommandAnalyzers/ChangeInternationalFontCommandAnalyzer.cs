using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using System;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class ChangeInternationalFontCommandAnalyzer : ZplCommandAnalyzerBase
    {

        public ChangeInternationalFontCommandAnalyzer() : base("^CI") { }

        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            // ^CIa,s1,d1,s2,d2,...: only the first parameter selects the encoding, the rest remap characters
            string charSet = zplCommand.Substring(this.PrinterCommandPrefix.Length).Split(',')[0].Trim();

            if (!Enum.TryParse(charSet, out InternationalFont encoding) || !Enum.IsDefined(encoding))
            {
                encoding = InternationalFont.ZCP850;
            }

            return new ZplChangeInternationalFont(encoding);
        }

    }
}
