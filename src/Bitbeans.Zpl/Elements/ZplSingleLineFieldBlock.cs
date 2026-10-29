namespace Bitbeans.Zpl.Elements
{
    /// <summary>A field block (<c>^FB</c>) with a maximum line count and line spacing of 9999, similar to <see cref="ZplTextField"/>, so that only the first line of text is visible. The text is wrapped at <c>width</c> and the remaining lines are pushed out of the label.</summary>
    public class ZplSingleLineFieldBlock : ZplFieldBlock
    {
        /// <summary>Creates a single-line field block.</summary>
        /// <param name="text">The text to print. Characters that are not allowed in field data are replaced, see <see cref="NewLineConversionMethod"/> and <paramref name="hexadecimalIndicator"/>. <see langword="null"/> renders empty field data.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Block width in dots; text beyond it is moved to (invisible) following lines.</param>
        /// <param name="font">Font (<c>^A</c>) rendered in front of the field.</param>
        /// <param name="textJustification">Justification of the line; default <see cref="TextJustification.Left"/>.</param>
        /// <param name="newLineConversion">How line feeds are converted; default <see cref="NewLineConversionMethod.ToSpace"/>.</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        public ZplSingleLineFieldBlock(
            string text,
            int positionX,
            int positionY,
            int width,
            ZplFont font,
            TextJustification textJustification = TextJustification.Left,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToSpace,
            char? hexadecimalIndicator = null,
            bool reversePrint = false)
            : base(text, positionX, positionY, width, font, 9999, 9999, textJustification, 0, newLineConversion, hexadecimalIndicator, reversePrint)
        {
        }
    }
}
