using Bitbeans.Zpl.Helpers;
using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Text block (<c>^TB</c>) with defined width and height and automatic word wrap. Text exceeding the block height is truncated. Line feeds are not supported, use <see cref="ZplFieldBlock"/> for multi-line text.</summary>
    public class ZplTextBlock : ZplTextField
    {
        /// <summary>Block width in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }

        /// <summary>Block height in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; private set; }

        /// <summary>Creates a text block.</summary>
        /// <param name="text">The text to print. Characters that are not allowed in field data are replaced, see <see cref="NewLineConversionMethod"/> and <paramref name="hexadecimalIndicator"/>. <see langword="null"/> renders empty field data.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Width in dots; scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Block height in dots.</param>
        /// <param name="font">Font (<c>^A</c>) rendered in front of the field.</param>
        /// <param name="newLineConversion">How line feeds are converted; <see cref="NewLineConversionMethod.ToZplNewLine"/> is not supported.</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <exception cref="NotSupportedException"><paramref name="newLineConversion"/> is <see cref="NewLineConversionMethod.ToZplNewLine"/>.</exception>
        public ZplTextBlock(
            string text,
            int positionX,
            int positionY,
            int width,
            int height,
            ZplFont font,
            NewLineConversionMethod newLineConversion = NewLineConversionMethod.ToSpace,
            char? hexadecimalIndicator = null)
            : base(text, positionX, positionY, font, newLineConversion, hexadecimalIndicator)
        {
            Width = width;
            Height = height;

            if (newLineConversion == NewLineConversionMethod.ToZplNewLine)
            {
                throw new NotSupportedException("ToZplNewLine is not supported, use ZplFieldBlock");
            }
        }

        /// <summary>Renders the font (<c>^A</c>), the position, <c>^TBo,w,h</c> and the field data. The orientation is taken from <see cref="ZplTextField.Font"/>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the text block.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            List<string> result = new List<string>();
            result.AddRange(Font.Render(context));
            result.AddRange(RenderPosition(context));
            result.Add($"^TB{RenderFieldOrientation(Font.FieldOrientation)},{context.Scale(Width)},{context.Scale(Height)}");
            result.Add(RenderFieldDataSection());

            return result;
        }
    }
}
