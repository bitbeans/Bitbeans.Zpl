using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>QR Code bar code (<c>^BQ</c>) with the field position and field data.</summary>
    /// <remarks>
    /// The field data is rendered as <c>^FD</c> followed by the error correction level, <c>A</c> (automatic input mode), a comma and the content. <c>^BQ</c> only supports the normal orientation, so <c>N</c> is always written.
    /// </remarks>
    public class ZplQrCode : ZplFieldDataElementBase
    {
        /// <summary>QR Code model (<c>b</c> parameter): 1 (original) or 2 (enhanced, recommended).</summary>
        public int Model { get; private set; }
        /// <summary>Magnification factor (<c>c</c> parameter, 1 to 10), the size of one module in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered (rounded, limited to 1 to 10).</summary>
        public int MagnificationFactor { get; private set; }
        /// <summary>Error correction level, written as first character of the field data.</summary>
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; private set; }
        /// <summary>Mask value (<c>e</c> parameter), 0 to 7; default 7.</summary>
        public int MaskValue { get; private set; }
        /// <summary>The vertical quiet zone in dots. Used only by the viewer (it is taken from the <c>^BY</c> height when a QR code is parsed); it is not written to ZPL.</summary>
        public int VerticalQuietZone { get; private set; }

        /// <summary>Creates a QR Code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="model">1 (original) or 2 (enhanced, recommended); default 2.</param>
        /// <param name="magnificationFactor">Size of one module in dots, 1-10; default 2. Scaled when rendered.</param>
        /// <param name="errorCorrectionLevel">Error correction level; default <see cref="ErrorCorrectionLevel.HighReliability"/> (Q).</param>
        /// <param name="maskValue">Mask value, 0-7; default 7.</param>
        /// <param name="fieldOrientation">Field orientation; <c>^BQ</c> only supports <see cref="FieldOrientation.Normal"/>.</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to emit <c>^FH</c> only when the content contains <c>^</c> or <c>~</c>. When set, the indicator character in the content is escaped too. Must be a printable ASCII character other than <c>^</c> and <c>~</c> (otherwise <see cref="System.ArgumentException"/>). The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="verticalQuietZone">The vertical quiet zone in dots; only used by the viewer (taken from the <c>^BY</c> height), not written to ZPL.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (bars/modules and interpretation line invert the underlying image).</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c> (<c>0</c> left, <c>1</c> right, <c>2</c> auto); <see cref="FieldJustification.None"/> omits it. A right justified field ends at the position.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="model"/> is not 1 or 2, <paramref name="magnificationFactor"/> is not 1 to 10, or <paramref name="maskValue"/> is not 0 to 7.</exception>
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
            int verticalQuietZone = 10,
            bool reversePrint = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition, reversePrint, fieldJustification)
        {
            if (fieldOrientation != FieldOrientation.Normal)
            {
                throw new ArgumentException("QR Codes (^BQ) only support the normal orientation.", nameof(fieldOrientation));
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(magnificationFactor, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(magnificationFactor, 10);

            ArgumentOutOfRangeException.ThrowIfLessThan(model, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(model, 2);
            ArgumentOutOfRangeException.ThrowIfLessThan(maskValue, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(maskValue, 7);

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
            result.Add($"^BQN,{Model},{context.ScaleModuleWidth(MagnificationFactor)},{RenderErrorCorrectionLevel(ErrorCorrectionLevel)},{MaskValue}");
            result.Add(RenderFieldDataSection($"{RenderErrorCorrectionLevel(ErrorCorrectionLevel)}A,"));

            return result;
        }
    }
}
