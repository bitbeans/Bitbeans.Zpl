using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>A bitmap font provider without any fonts; <see cref="TryGet"/> always fails.</summary>
    public sealed class EmptyZplBitmapFontProvider : IZplBitmapFontProvider
    {
        /// <summary>Gets the shared instance.</summary>
        public static EmptyZplBitmapFontProvider Instance { get; } = new EmptyZplBitmapFontProvider();

        private EmptyZplBitmapFontProvider()
        {
        }

        ///<inheritdoc/>
        public bool TryGet(
            string fontName,
            int printDensityDpmm,
            InternationalFont internationalFont,
            out ZplBitmapFontData font)
        {
            font = null;
            return false;
        }
    }
}

