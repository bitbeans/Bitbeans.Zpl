using Bitbeans.Zpl.Helpers;
using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Field block (<c>^FB</c>). Prints text in a block with automatic word wrap, a maximum number of lines, line spacing and justification. Line feeds are converted to <c>\&amp;</c> by default.</summary>
    public class ZplFieldBlock : ZplTextField
    {
        /// <summary>Block width in dots (<c>w</c> parameter of <c>^FB</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }

        /// <summary>Maximum number of lines in the block (<c>l</c> parameter of <c>^FB</c>, 1 to 9999). Surplus text overwrites the last line.</summary>
        public int MaxLineCount { get; private set; }

        /// <summary>Additional space between lines in dots (<c>s</c> parameter of <c>^FB</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int LineSpace { get; private set; }

        /// <summary>Justification of the text lines (<c>j</c> parameter of <c>^FB</c>).</summary>
        public TextJustification TextJustification { get; private set; }

        /// <summary>Hanging indent in dots of the second and all following lines (<c>h</c> parameter of <c>^FB</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int HangingIndent { get; private set; }

        /// <summary>Creates a field block.</summary>
        /// <param name="text">The text to print. Characters that are not allowed in field data are replaced, see <see cref="NewLineConversionMethod"/> and <paramref name="hexadecimalIndicator"/>. <see langword="null"/> renders empty field data.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Block width in dots.</param>
        /// <param name="font">Font (<c>^A</c>) rendered in front of the field.</param>
        /// <param name="maxLineCount">Maximum number of lines (1-9999); default 1.</param>
        /// <param name="lineSpace">Additional space between lines in dots; default 0.</param>
        /// <param name="textJustification">Justification of the text lines; default <see cref="TextJustification.Left"/>.</param>
        /// <param name="hangingIndent">Hanging indent in dots of the second and following lines; default 0.</param>
        /// <param name="newLineConversion">How line feeds are converted; default <see cref="NewLineConversionMethod.ToZplNewLine"/> (<c>\&amp;</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplFieldBlock(
            string text,
            int positionX,
            int positionY,
            int width,
            ZplFont font,
            int maxLineCount = 1,
            int lineSpace = 0,
            TextJustification textJustification = TextJustification.Left,
            int hangingIndent = 0,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToZplNewLine,
            char? hexadecimalIndicator = null,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false)
            : base(text, positionX, positionY, font, newLineConversion, hexadecimalIndicator, reversePrint, bottomToTop, useDefaultPosition: useDefaultPosition)
        {
            TextJustification = textJustification;
            Width = width;
            MaxLineCount = maxLineCount;
            LineSpace = lineSpace;
            HangingIndent = hangingIndent;
        }

        private string RenderTextJustification()
        {
            switch (TextJustification)
            {
                case TextJustification.Left:
                    return "L";
                case TextJustification.Center:
                    return "C";
                case TextJustification.Right:
                    return "R";
                case TextJustification.Justified:
                    return "J";
            }

            throw new NotImplementedException("Unknown Text Justification");
        }

        /// <summary>Renders the font (<c>^A</c>), the position, <c>^FBw,l,s,j,h</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the field block.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^ XA
            //^ CF0,30,30 ^ FO25,50
            //   ^ FB250,4,,
            //^ FDFD command that IS\&
            // preceded by an FB \&command.
            // ^ FS
            // ^ XZ
            List<string> result = new List<string>();
            result.AddRange(Font.Render(context));
            result.AddRange(RenderPosition(context));
            result.Add($"^FB{context.Scale(Width)},{MaxLineCount},{context.Scale(LineSpace)},{RenderTextJustification()},{context.Scale(HangingIndent)}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
