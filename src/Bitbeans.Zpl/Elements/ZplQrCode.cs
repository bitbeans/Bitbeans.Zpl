using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>QR Code bar code (<c>^BQ</c>) with the field position and field data.</summary>
    /// <remarks>
    /// The field data is rendered as <c>^FD</c> followed by the error correction level, <c>A</c> (automatic input mode), a comma and the content.
    /// </remarks>
    public class ZplQrCode : ZplFieldDataElementBase
    {
        /// <summary>QR Code model (<c>b</c> parameter): 1 (original) or 2 (enhanced, recommended).</summary>
        public int Model { get; private set; }
        /// <summary>Magnification factor (<c>c</c> parameter, 1 to 10), the size of one module in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int MagnificationFactor { get; private set; }
        /// <summary>Error correction level, written as first character of the field data.</summary>
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; private set; }
        /// <summary>Mask value (<c>e</c> parameter), 0 to 7; default 7.</summary>
        public int MaskValue { get; private set; }
        /// <summary>The vertical quiet zone in dots (usually set with <c>^BY,,X</c>). Currently not rendered.</summary>
        public int VerticalQuietZone { get; private set; }

        /// <summary>Creates a QR Code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="model">1 (original) or 2 (enhanced, recommended); default 2.</param>
        /// <param name="magnificationFactor">Size of one module in dots, 1-10; default 2.</param>
        /// <param name="errorCorrectionLevel">Error correction level; default <see cref="ErrorCorrectionLevel.HighReliability"/> (Q).</param>
        /// <param name="maskValue">Mask value, 0-7; default 7.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="verticalQuietZone">The vertical quiet zone (usually set with <c>^BY,,X</c>); stored but currently not rendered.</param>
        public ZplQrCode(
            string content,
            int positionX,
            int positionY,
            int model = 2,
            int magnificationFactor = 2,
            ErrorCorrectionLevel errorCorrectionLevel = ErrorCorrectionLevel.HighReliability,
            int maskValue = 7,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            int verticalQuietZone = 10)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition)
        {
            Model = model;
            MagnificationFactor = magnificationFactor;
            ErrorCorrectionLevel = errorCorrectionLevel;
            MaskValue = maskValue;
            VerticalQuietZone = verticalQuietZone;
        }

        /// <summary>Renders the position, <c>^BQo,model,magnification,ecc,mask</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ FO100,100
            //^ BQN,2,10
            //^ FDMM,AAC - 42 ^ FS
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^BQ{RenderFieldOrientation()},{Model},{context.Scale(MagnificationFactor)},{RenderErrorCorrectionLevel(ErrorCorrectionLevel)},{MaskValue}");
            result.Add(RenderFieldDataSection());

            return result;
        }

        /// <summary>Renders the field data section: optional <c>^FH</c>, then <c>^FD</c>, the error correction letter, <c>A,</c>, the content and <c>^FS</c>. Hides <see cref="ZplFieldDataElementBase.RenderFieldDataSection"/>.</summary>
        /// <returns>The field data section as a single line.</returns>
        protected new string RenderFieldDataSection()
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

            sb.Append($"^FD{RenderErrorCorrectionLevel(ErrorCorrectionLevel)}A,{Content}^FS");

            return sb.ToString();
        }
    }
}
