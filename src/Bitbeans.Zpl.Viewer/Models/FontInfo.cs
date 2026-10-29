using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The font set with <c>^A</c> for the next field.</summary>
    public class FontInfo
    {
        /// <summary>Gets the font name (<c>f</c> parameter of <c>^A</c>).</summary>
        public string FontName { get; private set; }
        /// <summary>Gets the field orientation (<c>o</c> parameter of <c>^A</c>).</summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>Gets the character width in dots (<c>w</c> parameter of <c>^A</c>).</summary>
        public int FontWidth { get; private set; }
        /// <summary>Gets the character height in dots (<c>h</c> parameter of <c>^A</c>).</summary>
        public int FontHeight { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="FontInfo"/> class.</summary>
        /// <param name="fontName">The ZPL font name (for example <c>A</c> or <c>0</c>).</param>
        /// <param name="fieldOrientation">The field orientation.</param>
        /// <param name="fontWidth">The character width in dots.</param>
        /// <param name="fontHeight">The character height in dots.</param>
        public FontInfo(string fontName, FieldOrientation fieldOrientation, int fontWidth, int fontHeight)
        {
            this.FontName = fontName;
            this.FieldOrientation = fieldOrientation;
            this.FontWidth = fontWidth;
            this.FontHeight = fontHeight;
        }
    }
}
