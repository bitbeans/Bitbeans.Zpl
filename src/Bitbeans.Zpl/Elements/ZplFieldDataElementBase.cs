using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of elements that consist of a field position and one field data (<c>^FD</c>/<c>^FS</c>) section, such as most bar codes. Used on its own it renders only the position and the field data.</summary>
    public class ZplFieldDataElementBase : ZplPositionedElementBase, IFormatElement
    {

        /// <summary>The field data. Written to <c>^FD</c>; can be replaced when merging a template. The characters <c>^</c> and <c>~</c> are written as hexadecimal escapes (<c>^FH</c> is added automatically with the default indicator <c>_</c> if no <see cref="HexadecimalIndicator"/> is set), so the content can never inject ZPL commands.</summary>
        public string Content { get; protected set; }

        /// <summary>Orientation of the field.</summary>
        public FieldOrientation FieldOrientation { get; protected set; }

        /// <summary>Hexadecimal escape character of <c>^FH</c>, or <see langword="null"/> if <c>^FH</c> is only emitted when the <see cref="Content"/> contains <c>^</c> or <c>~</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>. When set, the indicator character itself in the <see cref="Content"/> is escaped too.</summary>
        public char? HexadecimalIndicator { get; protected set; }

        /// <summary>Initializes the element.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only if the content contains <c>^</c> or <c>~</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>. Must be a printable ASCII character other than <c>^</c> and <c>~</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <exception cref="System.ArgumentException"><paramref name="hexadecimalIndicator"/> is <c>^</c>, <c>~</c>, a control character or not printable ASCII.</exception>
        public ZplFieldDataElementBase(
            string content,
            int positionX,
            int positionY,
            FieldOrientation fieldOrientation,
            char? hexadecimalIndicator,
            bool bottomToTop,
            bool useDefaultPosition)
            : base(positionX, positionY, bottomToTop, useDefaultPosition)
        {
            Content = content;
            FieldOrientation = fieldOrientation;
            HexadecimalIndicator = ZplHexadecimal.Validate(hexadecimalIndicator, nameof(hexadecimalIndicator));
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add(RenderFieldDataSection());

            return result;
        }

        /// <summary>Renders <see cref="FieldOrientation"/> as ZPL letter.</summary>
        /// <returns><c>N</c>, <c>R</c>, <c>I</c> or <c>B</c>.</returns>
        protected string RenderFieldOrientation()
        {
            return RenderFieldOrientation(FieldOrientation);
        }

        /// <summary>Renders the field data section: optional <c>^FH</c>, then <c>^FD</c>, the <see cref="Content"/> and <c>^FS</c>. <c>^</c> and <c>~</c> (and the hexadecimal indicator) in the content are escaped; <c>^FH_</c> is added automatically if needed.</summary>
        /// <returns>The field data section as a single line.</returns>
        protected string RenderFieldDataSection()
        {
            return RenderFieldDataSection(string.Empty);
        }

        /// <summary>Renders the field data section with a fixed, unescaped prefix in front of the escaped <see cref="Content"/>.</summary>
        /// <param name="dataPrefix">Prefix of the field data that is written unchanged, e.g. the <c>QA,</c> of a QR code.</param>
        /// <returns>The field data section as a single line.</returns>
        protected string RenderFieldDataSection(string dataPrefix)
        {
            string content = Content ?? string.Empty;
            char? indicator = HexadecimalIndicator;
            if (indicator == null && ZplHexadecimal.ContainsCommandPrefix(content))
            {
                indicator = ZplHexadecimal.DefaultIndicator;
            }

            var sb = new StringBuilder();
            if (indicator is char hexIndicator)
            {
                sb.Append(ZplHexadecimal.RenderCommand(hexIndicator));
                sb.Append("^FD").Append(dataPrefix);
                foreach (char c in content)
                {
                    sb.Append(ZplHexadecimal.MustEscape(c, hexIndicator) ? ZplHexadecimal.Escape(c, hexIndicator) : c.ToString());
                }
            }
            else
            {
                sb.Append("^FD").Append(dataPrefix).Append(content);
            }

            sb.Append("^FS");

            return sb.ToString();
        }

        /// <inheritdoc />
        public void SetTemplateContent(string content)
        {
            Content = content;
        }

    }
}
