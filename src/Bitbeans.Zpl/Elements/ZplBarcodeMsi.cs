using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements;

/// <summary>MSI bar code (<c>^BM</c>), rendered together with <c>^BY</c> and the field position.</summary>
public class ZplBarcodeMsi : ZplBarcode
{
    /// <summary>Check digit selection (<c>e</c> parameter of <c>^BM</c>).</summary>
    public MsiBarcodeCheckDigitMode CheckDigitMode { get; private set; }
    /// <summary>Print the check digit(s) in the interpretation line (last parameter of <c>^BM</c>).</summary>
    public bool PrintCheckDigit { get; private set; }

    /// <summary>Creates an MSI bar code.</summary>
    /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
    /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
    /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
    /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
    /// <param name="checkDigitMode">Check digit selection: <c>A</c> (none), <c>B</c> (Modulus 10), <c>C</c> (two Modulus 10) or <c>D</c> (Modulus 11 and Modulus 10).</param>
    /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
    /// <param name="printInterpretationLine"><see langword="true"/> to print the human-readable interpretation line (<c>Y</c>), <see langword="false"/> to suppress it (<c>N</c>).</param>
    /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it.</param>
    /// <param name="printCheckDigit">Print the check digit in the interpretation line; default <see langword="false"/>.</param>
    /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
    /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
    public ZplBarcodeMsi(
        string content,
        int positionX,
        int positionY,
        int height,
        int moduleWidth,
        double wideBarToNarrowBarWidthRatio,
        FieldOrientation fieldOrientation,
        MsiBarcodeCheckDigitMode checkDigitMode,
        char? hexadecimalIndicator,
        bool printInterpretationLine = true,
        bool printInterpretationLineAboveCode = false,
        bool printCheckDigit = false,
        bool bottomToTop = false,
        bool useDefaultPosition = false) :
        base(
            content,
            positionX,
            positionY,
            height,
            moduleWidth,
            wideBarToNarrowBarWidthRatio,
            fieldOrientation,
            hexadecimalIndicator,
            printInterpretationLine,
            printInterpretationLineAboveCode,
            bottomToTop,
            useDefaultPosition)
    {
        CheckDigitMode = checkDigitMode;
        PrintCheckDigit = printCheckDigit;
    }

    /// <summary>Renders <c>^FO</c>/<c>^FT</c>, <c>^BY</c>, <c>^BMo,e,h,f,g,e</c> and the field data.</summary>
    /// <param name="context">Render options (scaling).</param>
    /// <returns>The ZPL lines of the bar code.</returns>
    public override IEnumerable<string> Render(ZplRenderOptions context)
    {
        List<string> result = [];
        result.AddRange(RenderPosition(context));
        result.Add(RenderModuleWidth(context));
        result.Add($"^BM{RenderFieldOrientation()},{RenderPrintCheckDigitMode()},{context.Scale(Height)},{RenderPrintInterpretationLine()},{RenderPrintInterpretationLineAboveCode()},{RenderBoolean(PrintCheckDigit)}");
        result.Add(RenderFieldDataSection());
        return result;
    }

    private string RenderPrintCheckDigitMode()
    {
        return CheckDigitMode switch
        {
            MsiBarcodeCheckDigitMode.None => "A",
            MsiBarcodeCheckDigitMode.Mod2_10 => "C",
            MsiBarcodeCheckDigitMode.Mod_1_11_1_10 => "D",
            _ => "B"
        };
    }
}
