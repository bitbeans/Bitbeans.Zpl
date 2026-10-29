using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    /// <summary>Base class of the command analyzers: matches a command by its prefix and provides helpers to split the parameters and convert common ZPL parameter values.</summary>
    public abstract class ZplCommandAnalyzerBase : IZplCommandAnalyzer
    {
        /// <summary>Gets the command prefix (for example <c>^BC</c>) handled by this analyzer.</summary>
        public string PrinterCommandPrefix { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="ZplCommandAnalyzerBase"/> class.</summary>
        /// <param name="prefix">The command prefix (for example <c>^FO</c>) this analyzer handles.</param>
        public ZplCommandAnalyzerBase(string prefix)
        {
            this.PrinterCommandPrefix = prefix;
        }

        ///<inheritdoc/>
        public bool CanAnalyze(string zplLine)
        {
            return zplLine.StartsWith(this.PrinterCommandPrefix);
        }

        ///<inheritdoc/>
        public abstract ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage);

        /// <summary>Removes the command prefix and splits the remaining parameters at commas.</summary>
        /// <param name="zplCommand">The complete ZPL command including its prefix and parameters.</param>
        /// <param name="dataStartIndex">Number of characters after the command prefix that are skipped before the parameters start.</param>
        /// <returns>The comma separated parameters; at least one entry, which is empty if no parameters were given.</returns>
        protected string[] SplitCommand(string zplCommand, int dataStartIndex = 0)
        {
            string zplCommandData = zplCommand.Substring(this.PrinterCommandPrefix.Length + dataStartIndex);
            return zplCommandData.Trim().Split(',');
        }

        /// <summary>Converts an orientation parameter (<c>N</c>, <c>R</c>, <c>I</c> or <c>B</c>) to a <see cref="FieldOrientation"/>.</summary>
        /// <param name="fieldOrientation">The orientation parameter.</param>
        /// <param name="virtualPrinter">The virtual printer holding the current printer state.</param>
        /// <returns>The matching orientation, or the current default orientation (<c>^FW</c>) of the virtual printer for any other value.</returns>
        protected FieldOrientation ConvertFieldOrientation(string fieldOrientation, VirtualPrinter virtualPrinter)
        {
            return fieldOrientation switch
            {
                "N" => FieldOrientation.Normal,
                "R" => FieldOrientation.Rotated90,
                "I" => FieldOrientation.Rotated180,
                "B" => FieldOrientation.Rotated270,
                _ => virtualPrinter.FieldOrientation,
            };
        }

        /// <summary>Converts a quality parameter (<c>0</c>, <c>50</c>, <c>80</c>, <c>100</c>, <c>140</c> or <c>200</c>) to a <see cref="QualityLevel"/>.</summary>
        /// <param name="qualityLevel">The quality parameter.</param>
        /// <returns>The matching quality level; <see cref="QualityLevel.ECC0"/> for any other value.</returns>
        protected QualityLevel ConvertQualityLevel(string qualityLevel)
        {
            return qualityLevel switch
            {
                "0" => QualityLevel.ECC0,
                "50" => QualityLevel.ECC50,
                "80" => QualityLevel.ECC80,
                "100" => QualityLevel.ECC100,
                "140" => QualityLevel.ECC140,
                "200" => QualityLevel.ECC200,
                _ => QualityLevel.ECC0
            };
        }

        /// <summary>Converts a justification parameter (<c>0</c>, <c>1</c> or <c>2</c>) to a <see cref="FieldJustification"/>.</summary>
        /// <param name="fieldJustification">The justification parameter.</param>
        /// <param name="virtualPrinter">The virtual printer holding the current printer state.</param>
        /// <returns>The matching justification, or the current default justification (<c>^FW</c>) of the virtual printer for any other value.</returns>
        protected FieldJustification ConvertFieldJustification(string fieldJustification,VirtualPrinter virtualPrinter)
        {
            return fieldJustification switch
            {
                "0" => FieldJustification.Left,
                "1" => FieldJustification.Right,
                "2" => FieldJustification.Auto,
                _ => virtualPrinter.FieldJustification,
            };
        }

        /// <summary>Gets the justification of the pending field: the justification given with <c>^FO</c>/<c>^FT</c>, otherwise the default justification set with <c>^FW</c>.</summary>
        /// <param name="virtualPrinter">The virtual printer holding the current printer state.</param>
        /// <returns>The justification, <see cref="FieldJustification.None"/> if none was set.</returns>
        protected static FieldJustification ResolveFieldJustification(VirtualPrinter virtualPrinter)
        {
            FieldJustification fieldJustification = virtualPrinter.NextElementFieldJustification;
            if (fieldJustification == FieldJustification.None)
            {
                fieldJustification = virtualPrinter.FieldJustification;
            }

            return fieldJustification;
        }

        /// <summary>Converts an error correction parameter (<c>H</c>, <c>Q</c>, <c>M</c> or <c>L</c>) to an <see cref="ErrorCorrectionLevel"/>.</summary>
        /// <param name="errorCorrection">The error correction parameter.</param>
        /// <returns>The matching level; <see cref="ErrorCorrectionLevel.Standard"/> for any other value.</returns>
        protected ErrorCorrectionLevel ConvertErrorCorrectionLevel(string errorCorrection)
        {
            return errorCorrection switch
            {
                "H" => ErrorCorrectionLevel.UltraHighReliability,
                "Q" => ErrorCorrectionLevel.HighReliability,
                "M" => ErrorCorrectionLevel.Standard,
                "L" => ErrorCorrectionLevel.HighDensity,
                _ => ErrorCorrectionLevel.Standard,
            };
        }

        /// <summary>Converts a <c>Y</c>/<c>N</c> parameter to a boolean.</summary>
        /// <param name="yesOrNo">The parameter value.</param>
        /// <param name="defaultValue">The value used when the parameter is null or empty.</param>
        /// <returns><see langword="true"/> if the parameter, or the default if the parameter is empty, is <c>Y</c>; otherwise <see langword="false"/>.</returns>
        protected bool ConvertBoolean(string yesOrNo, string defaultValue = "N")
        {
            return (!string.IsNullOrEmpty(yesOrNo) ? yesOrNo : defaultValue) == "Y";
        }

        /// <summary>Finds the zero-based index of the n-th occurrence of a character.</summary>
        /// <param name="input">The text to search.</param>
        /// <param name="occurranceToFind">The number of the occurrence to find (1 for the first).</param>
        /// <param name="charToFind">The character to search for.</param>
        /// <returns>The index of the occurrence, or -1 if there are fewer occurrences.</returns>
        protected int IndexOfNthCharacter(string input, int occurranceToFind, char charToFind)
        {
            int index = -1;
            for (int i = 0; i < occurranceToFind; i++)
            {
                index = input.IndexOf(charToFind, index + 1);

                if (index == -1)
                {
                    break;
                }
            }

            return index;
        }

        /// <summary>Converts the MSI check digit parameter (<c>A</c> to <c>D</c>) to a <see cref="MsiBarcodeCheckDigitMode"/>.</summary>
        /// <param name="checkDigitSelection">The check digit parameter.</param>
        /// <returns>The matching mode; <see cref="MsiBarcodeCheckDigitMode.Mod1_10"/> for any other value.</returns>
        protected MsiBarcodeCheckDigitMode ConvertCheckDigitSelection(string checkDigitSelection)
        {
            return checkDigitSelection switch
            {
                "A" => MsiBarcodeCheckDigitMode.None,
                "B" => MsiBarcodeCheckDigitMode.Mod1_10,
                "C" => MsiBarcodeCheckDigitMode.Mod2_10,
                "D" => MsiBarcodeCheckDigitMode.Mod_1_11_1_10,
                _ => MsiBarcodeCheckDigitMode.Mod1_10
            };
        }

        /// <summary>Converts the Code 11 check digit parameter to a <see cref="Code11CheckDigitCount"/>.</summary>
        /// <param name="checkDigitCount">The check digit parameter.</param>
        /// <returns><see cref="Code11CheckDigitCount.One"/> for <c>Y</c>, otherwise <see cref="Code11CheckDigitCount.Two"/>.</returns>
        protected Code11CheckDigitCount ConvertCode11CheckDigitCount(string checkDigitCount)
        {
            return checkDigitCount switch
            {
                "Y" => Code11CheckDigitCount.One,
                _ => Code11CheckDigitCount.Two
            };
        }
    }
}
