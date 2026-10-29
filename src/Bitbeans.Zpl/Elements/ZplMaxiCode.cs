using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>MaxiCode bar code (<c>^BD</c>) with the field position and field data. The orientation is not rendered.</summary>
    public class ZplMaxiCode : ZplFieldDataElementBase
    {
        /// <summary>MaxiCode mode (<c>m</c> parameter): 2 (numeric postal code), 3 (alphanumeric postal code), 4 (standard), 5 (full EEC) or 6 (reader programming).</summary>
        public int Mode { get; private set; }

        /// <summary>Position of this symbol in a structured append sequence (<c>n</c> parameter), 1 to 8.</summary>
        public int Position { get; private set; }

        /// <summary>Total number of symbols in a structured append sequence (<c>t</c> parameter), 1 to 8.</summary>
        public int Total { get; private set; }

        /// <summary>Creates a MaxiCode bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="mode">2 (numeric postal code, default), 3 (alphanumeric postal code), 4 (standard), 5 (full EEC) or 6 (reader programming).</param>
        /// <param name="position">Position in a structured append sequence, 1-8; default 1.</param>
        /// <param name="total">Total number of symbols in a structured append sequence, 1-8; default 1.</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplMaxiCode(
            string content,
            int positionX,
            int positionY,
            int mode = 2,
            int position = 1,
            int total = 1,
            char? hexadecimalIndicator = null,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(content, positionX, positionY, FieldOrientation.Normal, hexadecimalIndicator, bottomToTop, useDefaultPosition)
        {
            Mode = mode;
            Position = position;
            Total = total;
        }

        /// <summary>Renders the position, <c>^BDm,n,t</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^FO100,100
            //^BD2,1,1
            //^FH^FD002840100450000_5B)>_1E01_1D961Z00136071_1DUPSN_1D123X56_1D028_1D_1D001/001_1D011_1DN_1D_1DNEW YORK_1DNY_1E_04^FS
            List<string> result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^BD{Mode},{Position},{Total}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
