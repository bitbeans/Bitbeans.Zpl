using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Provides the data for a field number (<c>^FN</c>) of a recalled format: renders <c>^FNn</c> followed by <c>^FD</c>text<c>^FS</c>. See <see cref="ZplFieldNumber"/>.</summary>
    public class ZplRecallFieldNumber : ZplElementBase
    {
        /// <summary>The field number (<c>^FN</c> parameter).</summary>
        public int Number { get; private set; }
        /// <summary>The data for the field; <see langword="null"/> is rendered as empty data. Line feeds are replaced by a space. If the text contains <c>^</c> or <c>~</c>, <c>^FH</c> is added and <c>^</c>, <c>~</c> and <c>_</c> are written as hexadecimal escapes.</summary>
        public string Text { get; private set; }

        /// <summary>Creates the data for a field number.</summary>
        /// <param name="number">The field number to provide data for.</param>
        /// <param name="text">The data of the field.</param>
        public ZplRecallFieldNumber(int number, string text)
        {
            Number = number;
            Text = text;
        }

        /// <summary>Renders <c>^FNn</c> and <c>^FDtext^FS</c> as two lines.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            string text = Text ?? string.Empty;
            char? indicator = ZplHexadecimal.ContainsCommandPrefix(text) ? ZplHexadecimal.DefaultIndicator : null;

            StringBuilder sb = new StringBuilder();
            if (indicator is char hexIndicator)
            {
                sb.Append(ZplHexadecimal.RenderCommand(hexIndicator));
            }

            sb.Append("^FD");
            foreach (var c in text)
            {
                sb.Append(ZplTextField.SanitizeCharacter(c, NewLineConversionMethod.ToSpace, indicator));
            }

            var result = new List<string>();
            result.Add($"^FN{Number}");
            result.Add($"{sb}^FS");
            return result;
        }
    }
}
