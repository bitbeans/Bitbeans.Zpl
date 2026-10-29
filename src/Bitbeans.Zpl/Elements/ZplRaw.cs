using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    public class ZplRaw : ZplElementBase
    {
        public string RawContent { get; private set; }

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
