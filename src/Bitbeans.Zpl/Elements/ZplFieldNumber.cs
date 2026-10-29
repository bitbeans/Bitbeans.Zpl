using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Field number (<c>^FN</c>) as declared in stored formats (templates). Renders the position/font/bar code commands of the wrapped element, but with <c>^FNn^FS</c> instead of the field data. The data is provided by a <see cref="ZplRecallFieldNumber"/>.</summary>
    public class ZplFieldNumber : ZplElementBase
    {
        /// <summary>The field number (<c>^FN</c> parameter, 0 to 9999).</summary>
        public int Number { get; private set; }
        /// <summary>The element providing location, font or bar code settings.</summary>
        public ZplElementBase FormatElement { get; private set; }

        /// <summary>
        /// Field number with format element (without data).
        /// </summary>
        /// <param name="number">Field number from 0 to 9999</param>
        /// <param name="formatElement">Element providing location, font</param>
        /// <exception cref="ArgumentException">If element is not an <see cref="IFormatElement"/>
        /// (allowing its data to be set when merging formats)</exception>
        public ZplFieldNumber(int number, ZplElementBase formatElement)
        {
            if (!(formatElement is IFormatElement))
            {
                throw new ArgumentException("", nameof(formatElement));
            }

            Number = number;
            FormatElement = formatElement;
        }

        /// <summary>Renders the <see cref="FormatElement"/> and replaces its field data (<c>^FD...</c>) in the last line by <c>^FNn^FS</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the format element with the field number.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>(FormatElement.Render(context));

            // ^FN replaces ^FD inside the field of the format element: ^FO..^A..^FN1^FS
            var last = result[result.Count - 1];
            var fieldDataStart = last.IndexOf("^FD", StringComparison.Ordinal);
            if (fieldDataStart >= 0)
            {
                last = last.Substring(0, fieldDataStart);
            }

            result[result.Count - 1] = $"{last}^FN{Number}^FS";
            return result;
        }
    }
}
