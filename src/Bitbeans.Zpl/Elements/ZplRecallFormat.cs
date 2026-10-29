using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Recalls a stored format (<c>^XF</c>); the field data of the recalled format is provided by <see cref="ZplRecallFieldNumber"/> elements.</summary>
    public class ZplRecallFormat : ZplElementBase
    {
        /// <summary>Name of the stored format to recall, written unchanged after <c>^XF</c>, e.g. <c>R:LABEL.ZPL</c>.</summary>
        public string FormatName { get; private set; }

        /// <summary>Creates a recall format element.</summary>
        /// <param name="formatName">Name of the stored format.</param>
        public ZplRecallFormat(string formatName)
        {
            FormatName = formatName;
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
