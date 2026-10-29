using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Placeholder for recalling a stored format (<c>^XF</c>). Rendering is not implemented.</summary>
    public class ZplRecallFormat : ZplElementBase
    {
        /// <summary>Name of the stored format to recall.</summary>
        public string FormatName { get; private set; }

        /// <summary>Creates a recall format element.</summary>
        /// <param name="formatName">Name of the stored format.</param>
        public ZplRecallFormat(string formatName)
        {
            FormatName = formatName;
        }

        /// <summary>Not implemented; always throws.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>Never returns.</returns>
        /// <exception cref="InvalidOperationException">Always thrown.</exception>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            throw new InvalidOperationException();
        }
    }
}
