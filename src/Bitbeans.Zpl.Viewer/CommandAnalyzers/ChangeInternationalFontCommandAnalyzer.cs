using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using System;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Analyzes <c>^CI</c> (change international font/encoding) and returns an element that selects the character set. Only the first parameter is evaluated; unknown values fall back to code page 850.</summary>
    public class ChangeInternationalFontCommandAnalyzer : ZplCommandAnalyzerBase
    {

        /// <summary>Initializes a new instance of the <see cref="ChangeInternationalFontCommandAnalyzer"/> class for the <c>^CI</c> command.</summary>
        public ChangeInternationalFontCommandAnalyzer() : base("^CI") { }

        ///<inheritdoc/>
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
