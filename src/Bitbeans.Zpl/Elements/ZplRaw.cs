using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Raw ZPL code that is rendered unchanged.</summary>
    public class ZplRaw : ZplElementBase
    {
        /// <summary>The raw ZPL code.</summary>
        public string RawContent { get; private set; }

        /// <summary>Creates a raw ZPL element.</summary>
        /// <param name="rawZplCode">The ZPL code, rendered as one line.</param>
        public ZplRaw(string rawZplCode)
        {
            RawContent = rawZplCode;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { RawContent };
        }
    }
}
