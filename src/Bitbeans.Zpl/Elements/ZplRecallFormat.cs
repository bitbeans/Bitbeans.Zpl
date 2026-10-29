using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Recalls a stored format (<c>^XF</c>); the field data of the recalled format is provided by <see cref="ZplRecallFieldNumber"/> elements.</summary>
    public class ZplRecallFormat : ZplElementBase
    {
        /// <summary>Name of the stored format to recall, written after <c>^XF</c>, e.g. <c>R:LABEL.ZPL</c>: an optional device letter (<c>R</c>, <c>E</c>, <c>B</c>, <c>A</c> or <c>Z</c>) and colon, then the file name (no <c>^</c>, <c>~</c>, comma, further colon, whitespace or control characters).</summary>
        public string FormatName { get; private set; }

        /// <summary>Creates a recall format element.</summary>
        /// <param name="formatName">Name of the stored format, optionally with device prefix (<c>R:LABEL.ZPL</c>); see <see cref="FormatName"/> for the allowed characters.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="formatName"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="formatName"/> is empty or contains characters that would be interpreted as ZPL commands or separators.</exception>
        public ZplRecallFormat(string formatName)
        {
            FormatName = ZplIdentifier.Path(formatName, nameof(formatName));
        }

        /// <summary>Renders <c>^XF</c>, the <see cref="FormatName"/> and <c>^FS</c>, e.g. <c>^XFR:LABEL.ZPL^FS</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL line.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new List<string> { $"^XF{FormatName}^FS" };
        }
    }
}
