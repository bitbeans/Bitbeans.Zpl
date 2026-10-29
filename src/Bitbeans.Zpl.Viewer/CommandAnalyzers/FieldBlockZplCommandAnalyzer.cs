using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

using System;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    public class FieldBlockZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldBlockZplCommandAnalyzer() : base("^FB")        { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            string[] zplDataParts = this.SplitCommand(zplCommand);

            int tmpint;
            int widthOfTextBlockLine = 0;
            int maximumNumberOfLinesInTextBlock = 1;
            int addOrDeleteSpaceBetweenLines = 0;
            TextJustification textJustification = TextJustification.Left;
            int hangingIndentOfTheSecondAndRemainingLines = 0;

            if (int.TryParse(zplDataParts[0], out tmpint))
            {
                widthOfTextBlockLine = Math.Clamp(tmpint, 0, 32000);
            }

            if (zplDataParts.Length > 1 && int.TryParse(zplDataParts[1], out tmpint))
            {
                maximumNumberOfLinesInTextBlock = Math.Clamp(tmpint, 1, 9999);
            }

            if (zplDataParts.Length > 2 && int.TryParse(zplDataParts[2], out tmpint))
            {
                addOrDeleteSpaceBetweenLines = Math.Clamp(tmpint, -9999, 9999);
            }

            if (zplDataParts.Length > 3)
            {
                switch (zplDataParts[3])
                {
                    case "C":
                        textJustification = TextJustification.Center;
                        break;
                    case "R":
                        textJustification = TextJustification.Right;
                        break;
                    case "J":
                        textJustification = TextJustification.Justified;
                        break;
                }
            }

            if (zplDataParts.Length > 4 && int.TryParse(zplDataParts[4], out tmpint))
            {
                hangingIndentOfTheSecondAndRemainingLines = Math.Clamp(tmpint, 0, 9999);
            }

            virtualPrinter.SetNextElementFieldBlock(new FieldBlock
            {
                WidthOfTextBlockLine = widthOfTextBlockLine,
                MaximumNumberOfLinesInTextBlock = maximumNumberOfLinesInTextBlock,
                AddOrDeleteSpaceBetweenLines = addOrDeleteSpaceBetweenLines,
                TextJustification = textJustification,
                HangingIndentOfTheSecondAndRemainingLines = hangingIndentOfTheSecondAndRemainingLines
            });

            return null;
        }
    }
}
