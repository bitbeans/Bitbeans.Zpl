using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>PDF417 bar code (<c>^B7</c>) with the field position and field data.</summary>
    /// <remarks>
    /// Format: <c>^B7o,h,s,c,r,t</c>. Preceded by <c>^BYw</c> with the module width.
    /// </remarks>
    public class ZplPDF417 : ZplFieldDataElementBase
    {
        /// <summary>Height of the individual rows in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; protected set; }
        /// <summary>Module width in dots (<c>^BY</c> parameter <c>w</c>, 1 to 10); rendered as is, not scaled.</summary>
        public int ModuleWidth { get; protected set; }
        /// <summary>Number of data columns to encode (<c>c</c> parameter, 1 to 30). <see langword="null"/> lets the printer balance rows and columns automatically.</summary>
        public int? Columns { get; protected set; }
        /// <summary>Number of rows to encode (<c>r</c> parameter, 3 to 90). <see langword="null"/> lets the printer balance rows and columns automatically.</summary>
        public int? Rows { get; protected set; }
        /// <summary>Truncate the right row indicators and the stop pattern (<c>t</c> parameter).</summary>
        public bool Compact { get; protected set; }
        /// <summary>Security level (<c>s</c> parameter, 0 to 8): number of error detection and correction code words. 0 gives error detection only.</summary>
        public int SecurityLevel { get; protected set; }

        /// <summary>Creates a PDF417 bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Height of the individual rows in dots; default 8.</param>
        /// <param name="moduleWidth">Module width in dots (<c>^BY</c> parameter <c>w</c>, 1 to 10); default 2. Scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="columns">1-30: number of data columns to encode. <see langword="null"/> (default) balances rows and columns automatically.</param>
        /// <param name="rows">3-90: number of rows to encode. <see langword="null"/> (default) balances rows and columns automatically.</param>
        /// <param name="compact">Truncate the right row indicators and the stop pattern; default <see langword="false"/>.</param>
        /// <param name="securityLevel">0-8: number of error detection and correction code words. The default (0) provides error detection only; higher levels add error correction and increase the symbol size.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (bars/modules and interpretation line invert the underlying image).</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c> (<c>0</c> left, <c>1</c> right, <c>2</c> auto); <see cref="FieldJustification.None"/> omits it. A right justified field ends at the position.</param>
        public ZplPDF417(
            string content,
            int positionX,
            int positionY,
            int height = 8,
            int moduleWidth = 2,
            int? columns = null,
            int? rows = null,
            bool compact = false,
            int securityLevel = 0,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            bool reversePrint = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition, reversePrint, fieldJustification)
        {
            Height = height;
            ModuleWidth = moduleWidth;
            Columns = columns;
            Rows = rows;
            Compact = compact;
            SecurityLevel = securityLevel;
        }

        /// <summary>Renders the position, <c>^BYw</c>, <c>^B7o,h,s,c,r,t</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO100,100
            //^ BQN,2,10
            //^ FDMM,AAC - 42 ^ FS
            List<string> result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^BY{context.ScaleModuleWidth(ModuleWidth)}");
            result.Add($"^B7{RenderFieldOrientation()},{context.Scale(Height)},{SecurityLevel},{Columns},{Rows},{RenderBoolean(Compact)}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
