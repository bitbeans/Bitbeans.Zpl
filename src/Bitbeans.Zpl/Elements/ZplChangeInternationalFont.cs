using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Change international font/encoding (<c>^CI</c>).</summary>
    public class ZplChangeInternationalFont : ZplElementBase
    {
        /// <summary>The selected character set (parameter <c>a</c> of <c>^CI</c>).</summary>
        public InternationalFont InternationalFont { get; private set; }

        /// <summary>Creates a <c>^CI</c> command.</summary>
        /// <param name="internationalFont">The character set to select.</param>
        public ZplChangeInternationalFont(InternationalFont internationalFont)
        {
            this.InternationalFont = internationalFont;
        }

        /// <summary>Renders <c>^CIa</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The <c>^CI</c> command.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { $"^CI{(int)this.InternationalFont}" };
        }
    }

}
