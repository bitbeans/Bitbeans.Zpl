using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Aztec bar code (<c>^BO</c>) with the field position and field data.</summary>
    public class ZplAztecBarcode : ZplFieldDataElementBase
    {
        /// <summary>Magnification factor (<c>b</c> parameter of <c>^BO</c>), 1 to 10; not scaled by <see cref="ZplRenderOptions.ScaleFactor"/>.</summary>
        public int MagnificationFactor { get; protected set; }
        /// <summary>Extended channel interpretation (ECI) code indicator (<c>c</c> parameter).</summary>
        public bool ExtendedChannel { get; protected set; }
        /// <summary>Error control and symbol size/type indicator (<c>d</c> parameter): 0 default error correction, 1-99 minimum error correction percentage, 101-104 compact symbol with 1-4 layers, 201-232 full-range symbol with 2-32 layers, 300 simple Aztec "Rune".</summary>
        public int ErrorControl { get; protected set; }
        /// <summary>Menu symbol indicator (<c>e</c> parameter); the data is a menu command.</summary>
        public bool MenuSymbol { get; protected set; }
        /// <summary>Number of symbols for structured append (<c>f</c> parameter), 1 to 26.</summary>
        public int SymbolCount { get; protected set; }
        /// <summary>Optional ID field for structured append (<c>g</c> parameter), up to 24 alphanumeric characters.</summary>
        public string IdField { get; protected set; }

        /// <summary>Creates an Aztec bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="magnificationFactor">Magnification factor (symbol module size in dots), 1-10; default 2.</param>
        /// <param name="extendedChannel">Extended channel interpretation code indicator; default <see langword="false"/>.</param>
        /// <param name="errorControl">Error control and symbol size/type indicator; default 0.</param>
        /// <param name="menuSymbol">Menu symbol indicator; default <see langword="false"/>.</param>
        /// <param name="symbolCount">Number of symbols for structured append (1-26); default 1.</param>
        /// <param name="idField">Optional ID field for structured append; default <see langword="null"/> (empty).</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplAztecBarcode(
            string content,
            int positionX,
            int positionY,
            int magnificationFactor = 2,
            bool extendedChannel = false,
            int errorControl = 0,
            bool menuSymbol = false,
            int symbolCount = 1,
            string idField = null,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition)
        {
            this.MagnificationFactor = magnificationFactor;
            this.ExtendedChannel = extendedChannel;
            this.ErrorControl = errorControl;
            this.MenuSymbol = menuSymbol;
            this.SymbolCount = symbolCount;
            this.IdField = idField;
        }

        /// <summary>Renders the position, <c>^BOa,b,c,d,e,f,g</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^BO{RenderFieldOrientation(this.FieldOrientation)},{this.MagnificationFactor},{RenderBoolean(this.ExtendedChannel)}," +
                $"{this.ErrorControl},{RenderBoolean(this.MenuSymbol)},{this.SymbolCount},{this.IdField}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
