using Bitbeans.Zpl.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Text field. Renders the font (<c>^A</c>), the position (<c>^FO</c> or <c>^FT</c>) and the field data (<c>^FD</c>/<c>^FS</c>, optionally with <c>^FH</c> and <c>^FR</c>).</summary>
    public class ZplTextField : ZplPositionedElementBase, IFormatElement
    {
        /// <summary>Font (<c>^A</c>) of the text.</summary>
        public ZplFont Font { get; protected set; }
        /// <summary>Hexadecimal escape character of <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. When set, <c>_</c>, <c>^</c>, <c>~</c> and the indicator character in the text are written as hexadecimal escapes.</summary>
        public char? HexadecimalIndicator { get; protected set; }
        /// <summary>Print the field in reverse (<c>^FR</c>), inverting the underlying image.</summary>
        public bool ReversePrint { get; protected set; }

        /// <summary>How line feeds in <see cref="Text"/> are converted.</summary>
        public NewLineConversionMethod NewLineConversion { get; protected set; }
        /// <summary>The text printed in the field (<c>^FD</c>). The characters <c>^</c> and <c>~</c> cannot be part of field data and are replaced by a space, or, when <see cref="HexadecimalIndicator"/> is set, written as hexadecimal escapes (as are <c>_</c> and the indicator itself). A backslash is printed literally, except in field blocks (<see cref="ZplFieldBlock"/>) where it is written as <c>\\</c>.</summary>
        public string Text { get; protected set; }

        /// <summary>Creates a text field, rendered as <c>^A</c>, <c>^FO</c>/<c>^FT</c>, optional <c>^FH</c>/<c>^FR</c> and <c>^FD</c>.</summary>
        /// <param name="text">Original text content; control characters are converted.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="newLineConversion">How line feeds are converted; default <see cref="NewLineConversionMethod.ToSpace"/>.</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="reversePrint"><see langword="true"/> to emit <c>^FR</c> so the field is printed in reverse (inverts the underlying image).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c>; <see cref="FieldJustification.None"/> omits it.</param>
        /// <param name="useDefaultPositionX">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT,y</c> without the x coordinate so the field continues at the x position that follows the previous field. Ignored for <c>^FO</c> or if <paramref name="useDefaultPosition"/> is set.</param>
        /// <exception cref="ArgumentException"><paramref name="hexadecimalIndicator"/> is <c>^</c>, <c>~</c>, a control character or not printable ASCII.</exception>
        public ZplTextField(
            string text,
            int positionX,
            int positionY,
            ZplFont font,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToSpace,
            char? hexadecimalIndicator = null,
            bool reversePrint = false,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None,
            bool useDefaultPositionX = false)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification, useDefaultPositionX)
        {
            Text = text;
            Font = font;
            HexadecimalIndicator = ZplHexadecimal.Validate(hexadecimalIndicator, nameof(hexadecimalIndicator));
            NewLineConversion = newLineConversion;
            ReversePrint = reversePrint;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            List<string> result = new List<string>();
            result.AddRange(Font.Render(context));
            result.AddRange(RenderPosition(context));
            result.Add(RenderFieldDataSection());

            return result;
        }

        /// <summary>Renders the field data section: optional <c>^FH</c> and <c>^FR</c>, then <c>^FD</c>, the sanitized <see cref="Text"/> and <c>^FS</c>.</summary>
        /// <returns>The field data section as a single line.</returns>
        protected string RenderFieldDataSection()
        {
            StringBuilder sb = new();
            if (HexadecimalIndicator is char hexIndicator)
            {
                sb.Append(ZplHexadecimal.RenderCommand(hexIndicator));
            }
            if (ReversePrint)
            {
                sb.Append("^FR");
            }

            sb.Append("^FD");
            if (Text != null)
            {
                foreach (char c in Text)
                {
                    sb.Append(SanitizeCharacter(c, NewLineConversion, HexadecimalIndicator, EscapeBackslash));
                }
            }
            sb.Append("^FS");

            return sb.ToString();
        }

        /// <summary>Whether a backslash is written as <c>\\</c>. <see langword="false"/> for plain text fields, where a backslash is literal field data; field blocks (<c>^FB</c>) interpret <c>\&amp;</c> and <c>\\</c> and override this.</summary>
        protected virtual bool EscapeBackslash => false;

        internal static string SanitizeCharacter(
            char input,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToSpace,
            char? hexadecimalIndicator = null,
            bool escapeBackslash = false)
        {
            if (hexadecimalIndicator is char indicator)
            {
                //Convert to hex ('_' is always escaped, it is the default indicator)
                if (input == '_' || ZplHexadecimal.MustEscape(input, indicator))
                {
                    return ZplHexadecimal.Escape(input, indicator);
                }
            }
            else if (input == '^' || input == '~')
            {
                //The field data can be any printable character except those used as command prefixes(^ and ~).
                return " ";
            }

            if (input == '\\' && escapeBackslash)
            {
                //Inside ^FB a literal backslash is written as \\
                return @"\\";
            }

            if (input == '\n')
            {
                switch (newLineConversion)
                {
                    case NewLineConversionMethod.ToEmpty:
                        return "";
                    case NewLineConversionMethod.ToSpace:
                        return " ";
                    case NewLineConversionMethod.ToZplNewLine:
                        return @"\&";
                }
            }

            return input.ToString();
        }

        /// <inheritdoc />
        public void SetTemplateContent(string content)
        {
            Text = content;
        }
    }
}
