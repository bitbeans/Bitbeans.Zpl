using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Scalable/bitmapped font selection (<c>^A</c>) for the following text field.</summary>
    /// <remarks>
    /// Rendered as <c>^Afo,h,w</c>; height and width are in dots and scaled by <see cref="ZplRenderOptions.ScaleFactor"/>.
    /// </remarks>
    public class ZplFont : ZplElementBase
    {
        /// <summary>Font identifier: any font in the printer (built-in, downloaded, EPROM or stored fonts), <c>A</c> to <c>Z</c> and <c>0</c> to <c>9</c>. Default: <c>0</c>.</summary>
        public string FontName { get; private set; }
        /// <summary>Orientation of the text.</summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>Character width in dots (<c>w</c> parameter of <c>^A</c>); 0 keeps the proportion to the height.</summary>
        public int FontWidth { get; private set; }
        /// <summary>Character height in dots (<c>h</c> parameter of <c>^A</c>).</summary>
        public int FontHeight { get; private set; }

        /// <summary>Creates a font.</summary>
        /// <param name="fontWidth">Character width in dots; default 30.</param>
        /// <param name="fontHeight">Character height in dots; default 30.</param>
        /// <param name="fontName">Font identifier (<c>A</c>-<c>Z</c>, <c>0</c>-<c>9</c>); default <c>0</c>.</param>
        /// <param name="fieldOrientation">Orientation of the text; default <see cref="FieldOrientation.Normal"/>.</param>
        public ZplFont(
            int fontWidth = 30,
            int fontHeight = 30,
            string fontName = "0",
            FieldOrientation fieldOrientation = FieldOrientation.Normal)
        {
            FontName = fontName;
            FieldOrientation = fieldOrientation;
            FontWidth = fontWidth;
            FontHeight = fontHeight;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { $"^A{FontName}{RenderFieldOrientation(FieldOrientation)},{context.Scale(FontHeight)},{context.Scale(FontWidth)}" };
        }
    }
}
