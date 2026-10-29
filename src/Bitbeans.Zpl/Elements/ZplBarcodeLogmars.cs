using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements;

/// <summary>LOGMARS bar code (<c>^BL</c>), a Code 39 variant used by the U.S. Department of Defense. Always printed with a human-readable interpretation line.</summary>
public class ZplBarcodeLogmars : ZplBarcode
{
    /// <summary>Creates a LOGMARS bar code.</summary>
    /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
    /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
    /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
    /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
    /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
    /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it (<c>g</c> parameter of <c>^BL</c>).</param>
    /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
    /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
    /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
    public ZplBarcodeLogmars(
        string content,
        int positionX,
        int positionY,
        int height,
        int moduleWidth,
        double wideBarToNarrowBarWidthRatio,
        FieldOrientation fieldOrientation,
        bool printInterpretationLineAboveCode,
        char? hexadecimalIndicator = null,
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
            true, // printInterpretationLine
            printInterpretationLineAboveCode,
            bottomToTop,
            useDefaultPosition)
    {
    }

    /// <summary>Renders <c>^FO</c>/<c>^FT</c>, <c>^BY</c>, <c>^BLo,h,g</c> and the field data.</summary>
    /// <param name="context">Render options (scaling).</param>
    /// <returns>The ZPL lines of the bar code.</returns>
    public override IEnumerable<string> Render(ZplRenderOptions context)
    {
        List<string> result = [];
        result.AddRange(RenderPosition(context));
        result.Add(RenderModuleWidth(context));
        result.Add($"^BL{RenderFieldOrientation()},{context.Scale(Height)},{RenderPrintInterpretationLineAboveCode()}");
        result.Add(RenderFieldDataSection());
        return result;
    }
}
