using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of elements that consist of a field position and one field data (<c>^FD</c>/<c>^FS</c>) section, such as most bar codes. Used on its own it renders only the position and the field data.</summary>
    public class ZplFieldDataElementBase : ZplPositionedElementBase, IFormatElement
    {

        /// <summary>The field data. Written unchanged to <c>^FD</c>; can be replaced when merging a template.</summary>
        public string Content { get; protected set; }

        /// <summary>Orientation of the field.</summary>
        public FieldOrientation FieldOrientation { get; protected set; }

        /// <summary>Hexadecimal escape character of <c>^FH</c>, or <see langword="null"/> if <c>^FH</c> is not emitted. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</summary>
        public char? HexadecimalIndicator { get; protected set; }

        /// <summary>Initializes the element.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
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
            HexadecimalIndicator = hexadecimalIndicator;
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

        /// <summary>Renders the field data section: optional <c>^FH</c>, then <c>^FD</c>, the <see cref="Content"/> and <c>^FS</c>.</summary>
        /// <returns>The field data section as a single line.</returns>
        protected string RenderFieldDataSection()
        {
            var sb = new StringBuilder();
            if (HexadecimalIndicator is char hexIndicator)
            {
                sb.Append("^FH");
                if (hexIndicator != '_')
                {
                    sb.Append(hexIndicator);
                }
            }

            sb.Append("^FD");
            sb.Append(this.Content);
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
