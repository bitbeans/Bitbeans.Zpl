using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    public class ZplRecallFormat : ZplElementBase
    {
        public string FormatName { get; private set; }

        public ZplRecallFormat(string formatName)
        {
            FormatName = formatName;
        }

        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            throw new InvalidOperationException();
        }
    }
}
