using System;
using System.Text;


namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of the linear bar codes that share the <c>^BY</c> module settings, a height and an optional interpretation line.</summary>
    public abstract class ZplBarcode : ZplFieldDataElementBase
    {
        /// <summary>Module width in dots, the width of the narrowest bar (<c>w</c> parameter of <c>^BY</c>). Rendered as is, it is not scaled by <see cref="ZplRenderOptions.ScaleFactor"/>.</summary>
        public int ModuleWidth { get; protected set; }
        /// <summary>Ratio of the wide bar width to the narrow bar width (<c>r</c> parameter of <c>^BY</c>), 2.0 to 3.0 in steps of 0.1. Only variable-ratio symbologies such as Code 39 use it.</summary>
        public double WideBarToNarrowBarWidthRatio { get; protected set; }
        /// <summary>Bar code height in dots; scaled with <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; protected set; }
        /// <summary>Print the human-readable interpretation line.</summary>
        public bool PrintInterpretationLine { get; protected set; }
        /// <summary>Print the interpretation line above the bar code instead of below it.</summary>
        public bool PrintInterpretationLineAboveCode { get; protected set; }

        /// <summary>Initializes the bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Bar code height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="moduleWidth">Module width in dots, the width of the narrowest bar (<c>^BY</c> parameter <c>w</c>, 1 to 10).</param>
        /// <param name="wideBarToNarrowBarWidthRatio">Ratio of wide bar to narrow bar width (<c>^BY</c> parameter <c>r</c>), 2.0 to 3.0 in steps of 0.1. Only evaluated by variable-ratio symbologies such as Code 39.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="printInterpretationLine"><see langword="true"/> to print the human-readable interpretation line (<c>Y</c>), <see langword="false"/> to suppress it (<c>N</c>).</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the bar code instead of below it.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplBarcode(
            string content,
            int positionX,
            int positionY,
            int height,
            int moduleWidth,
            double wideBarToNarrowBarWidthRatio,
            FieldOrientation fieldOrientation,
            char? hexadecimalIndicator,
            bool printInterpretationLine,
            bool printInterpretationLineAboveCode,
            bool bottomToTop,
            bool useDefaultPosition)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition)
        {
            Height = height;
            ModuleWidth = moduleWidth;
            WideBarToNarrowBarWidthRatio = wideBarToNarrowBarWidthRatio;
            PrintInterpretationLine = printInterpretationLine;
            PrintInterpretationLineAboveCode = printInterpretationLineAboveCode;
        }

        /// <summary>Renders <see cref="PrintInterpretationLine"/> as <c>Y</c> or <c>N</c>.</summary>
        /// <returns><c>Y</c> or <c>N</c>.</returns>
        protected string RenderPrintInterpretationLine()
        {
            return RenderBoolean(PrintInterpretationLine);
        }

        /// <summary>Renders <see cref="PrintInterpretationLineAboveCode"/> as <c>Y</c> or <c>N</c>.</summary>
        /// <returns><c>Y</c> or <c>N</c>.</returns>
        protected string RenderPrintInterpretationLineAboveCode()
        {
            return RenderBoolean(PrintInterpretationLineAboveCode);
        }

        /// <summary>Renders the bar code field default command <c>^BYw,r</c> from <see cref="ModuleWidth"/> and <see cref="WideBarToNarrowBarWidthRatio"/> (culture invariant). The module width is scaled by <see cref="ZplRenderOptions.ScaleFactor"/>, rounded and limited to 1 to 10; the ratio is not scaled.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The <c>^BY</c> command.</returns>
        protected string RenderModuleWidth(ZplRenderOptions context)
        {
            return FormattableString.Invariant($"^BY{context.ScaleModuleWidth(ModuleWidth)},{WideBarToNarrowBarWidthRatio}");
        }

    }
}
