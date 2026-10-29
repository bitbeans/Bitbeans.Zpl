using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Font identifier (<c>^CW</c>). Assigns a single alphanumeric character to a font stored in DRAM, on a memory card, in EPROM or in flash, so it can be selected with <c>^A</c> like a built-in font.</summary>
    public class ZplFontIdentifier : ZplElementBase
    {
        /// <summary>The letter or digit that refers to the font (<c>a</c> parameter of <c>^CW</c>).</summary>
        public char FontReplaceLetter { get; private set; }
        /// <summary>Storage device of the font file (<c>d</c> parameter of <c>^CW</c>), e.g. <c>E</c>.</summary>
        public string Device { get; private set; }
        /// <summary>Name and extension of the font file (<c>o.x</c> parameters of <c>^CW</c>), e.g. <c>ARIAL.TTF</c>.</summary>
        public string FontFileName { get; private set; }

        /// <summary>Creates a font identifier.</summary>
        /// <param name="fontReplaceLetter">The letter or digit that will refer to the font.</param>
        /// <param name="device">Storage device letter without colon, e.g. <c>E</c>.</param>
        /// <param name="fontFileName">Font file name including extension.</param>
        public ZplFontIdentifier(char fontReplaceLetter, string device, string fontFileName)
        {
            FontReplaceLetter = fontReplaceLetter;
            Device = device;
            FontFileName = fontFileName;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^CWa,d:o.x
            var result = new List<string>
            {
                $"^CW{FontReplaceLetter},{Device}:{FontFileName}"
            };

            return result;
        }
    }
}
